using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.UI.Phone;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(WageManager.Core), "WageManager", "1.0.0", "raequ", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace WageManager
{
    public class Core : MelonMod
    {
        #region WageCosts
        private float _bungalowCosts;
        private float _docksCosts;
        private float _barnCosts;
        private float _sweatshopCosts;
        #endregion

        #region PropertyCodes
        private const string Bungalow = "bungalow";
        private const string DocksWarehouse = "dockswarehouse";
        private const string Barn = "barn";
        private const string Sweatshop = "sweatshop";
        #endregion

        #region GUIFields
        private bool _showMenu = false;
        private Rect _menuRect = new Rect(20, 20, 250, 300);
        private bool _wasPhoneOpen;
        #endregion

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("WageManager Initialized.");
        }

        public override void OnUpdate()
        {
            bool isPhoneOpen = Phone.Instance.IsOpen;
            if (isPhoneOpen != _wasPhoneOpen)
            {
                if (isPhoneOpen)
                {
                    GetWorkerWages();
                }
                _showMenu = !_showMenu;
            }
            _wasPhoneOpen = isPhoneOpen;
        }

        public override void OnGUI()
        {
            if (!_showMenu) return;

            GUI.skin.window.fontSize = 14;
            _menuRect = GUI.Window(0, _menuRect, (GUI.WindowFunction)DrawMenuWindow, "Employee Wages Manager");
        }

        void DrawMenuWindow(int windowID)
        {
            var propertyManager = PropertyManager.Instance;
            GUILayout.BeginVertical();


            // Display wage information
            GUILayout.Label($"Bungalow Wages: ${_bungalowCosts:F2}");
            GUILayout.Label($"Docks Wages: ${_docksCosts:F2}");
            GUILayout.Label($"Barn Wages: ${_barnCosts:F2}");
            GUILayout.Label($"Sweatshop Wages: ${_sweatshopCosts:F2}");

            GUILayout.Space(20);

            // Pay buttons for each property
            if (GUILayout.Button("Pay Bungalow Workers"))
            {
                PayWorkers(propertyManager.GetProperty(Bungalow));
            }

            if (GUILayout.Button("Pay Docks Workers"))
            {
                PayWorkers(propertyManager.GetProperty(DocksWarehouse));
            }

            if (GUILayout.Button("Pay Barn Workers"))
            {
                PayWorkers(propertyManager.GetProperty(Barn));
            }

            if (GUILayout.Button("Pay Sweatshop Workers"))
            {
                PayWorkers(propertyManager.GetProperty(Sweatshop));
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Close"))
            {
                _showMenu = false;
            }

            GUILayout.EndVertical();

            // Allow the window to be dragged
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void GetWorkerWages()
        {
            foreach (var property in Property.Properties)
            {
                if (!property.IsOwned) continue;
                var employees = property.Employees;
                float total = 0f;
                foreach (var employee in employees)
                {
                    total += employee.DailyWage;
                    LoggerInstance.Msg($"{property.PropertyCode} employee {employee.name} has wage of {employee.DailyWage}");
                }

                switch (property.PropertyCode)
                {
                    case Bungalow:
                        _bungalowCosts = total;
                        break;
                    case DocksWarehouse:
                        _docksCosts = total;
                        break;
                    case Barn:
                        _barnCosts = total;
                        break;
                    case Sweatshop:
                        _sweatshopCosts = total;
                        break;
                        // manor, rv, motelroom intentionally skipped
                }
                total = 0f;
            }
        }

        private void PayWorkers(Property property)
        {
            switch (property.PropertyCode)
            {
                case Bungalow:
                    Pay(_bungalowCosts, property.PropertyCode);
                    SetPaid(property);
                    break;
                case Barn:
                    Pay(_barnCosts, property.PropertyCode);
                    SetPaid(property);
                    break;
                case Sweatshop:
                    Pay(_sweatshopCosts, property.PropertyCode);
                    SetPaid(property);
                    break;
                case DocksWarehouse:
                    Pay(_docksCosts, property.PropertyCode);
                    SetPaid(property);
                    break;
                default:
                    Pay(0.0f, property.PropertyCode);
                    break;
            }




        }
        private void SetPaid(Property property)
        {
            foreach (var employee in property.Employees)
            {
                employee.SetIsPaid();
            }
        }
        private void Pay(float cost, string code)
        {
            var moneyManager = MoneyManager.Instance;
            if (moneyManager == null)
            {
                LoggerInstance.Msg("Failed to get moneyManager");
                return;
            }
            if (moneyManager.onlineBalance < cost)
            {
                return;
            }
            moneyManager.CreateOnlineTransaction($"{code}", -cost, 1, $"Paid employee wages at {code}");

        }
    }
}
