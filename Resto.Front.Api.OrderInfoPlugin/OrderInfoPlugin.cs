using Resto.Front.Api.Attributes;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.OrderInfoPlugin.Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Resto.Front.Api.OrderInfoPlugin {
    using static PluginContext;

    [PluginLicenseModuleId(21005108)]
    public class OrderInfoPlugin : IFrontPlugin {
        private CompositeDisposable subscriptions;
        private CompositeDisposable buttons;

        public OrderInfoPlugin() {
            Log.Info("Initializing OrderInfoPlugin");

            SubcribeOnEvents();
            CreateButtons();

            Operations.AddNotificationMessage(string.Format(OrderInfoPluginLocalResources.PluginStarted, nameof(OrderInfoPlugin)), nameof(OrderInfoPlugin));
            

            Log.Info("OrderInfoPlugin started");
        }

        public void CreateButtons() {
            buttons = new CompositeDisposable {
                AddButtonNumberOfPositions(),
            };
        }

        public void SubcribeOnEvents() {
            subscriptions = new CompositeDisposable {
                SubscribeOnDeliveryOrderCreated(x => Operations.AddNotificationMessage(string.Format(OrderInfoPluginLocalResources.CreatedDeliveryOrder, x.Number), nameof(OrderInfoPlugin))),
                SubscribeOnOrderCreated(x => Operations.AddNotificationMessage(string.Format(OrderInfoPluginLocalResources.CreatedOrder, x.Number), nameof(OrderInfoPlugin))),
                SubscribeOnCultureChanged()
            };
        }

        public IDisposable SubscribeOnCultureChanged() {
            return Notifications.CurrentCultureChanged.Subscribe(x => {
                CultureInfo.CurrentCulture = x.culture;
                CultureInfo.CurrentUICulture = x.uiCulture;
                CultureInfo.DefaultThreadCurrentCulture = x.culture;
                CultureInfo.DefaultThreadCurrentUICulture = x.uiCulture;

                //to update localization on UI elements you need to recreate them
                buttons?.Dispose();
                CreateButtons();
            });
        }

        public IDisposable SubscribeOnDeliveryOrderCreated(Action<IDeliveryOrder> action) {
            return Notifications
                    .DeliveryOrderChanged
                    .Where(x => x.EventType == Data.Common.EntityEventType.Created)
                    .Select(x => x.Entity)
                    .Subscribe(action);
        }

        public IDisposable SubscribeOnOrderCreated(Action<IOrder> action) {
            return Notifications
                    .OrderChanged
                    .Where(x => x.EventType == Data.Common.EntityEventType.Created)
                    .Select(x => x.Entity)
                    .Where(x => !(x is IDeliveryOrder)) //you need to filter delivered and regular orders
                    .Subscribe(action);
        }

        public IDisposable AddButtonNumberOfPositions() {
            return Operations
                    .AddButtonToOrderEditScreen(OrderInfoPluginLocalResources.NumberOfPositionsInOrderButton,
                        x => x.vm.ShowOkPopup(OrderInfoPluginLocalResources.NumberOfPositionsInOrderCaption,
                            string.Format(OrderInfoPluginLocalResources.NumberOfPositionsInOrderMessage, x.order.Items.Count)));
        }

        public void Dispose() {
            subscriptions?.Dispose();
            buttons?.Dispose();
        }
    }
}
