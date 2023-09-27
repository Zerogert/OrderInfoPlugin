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
        private readonly CompositeDisposable subscriptions;

        public OrderInfoPlugin() {
            Log.Info("Initializing OrderInfoPlugin");

            subscriptions = new CompositeDisposable {
                AddButtonNumberOfPositions(),

                Notifications
                    .OrderChanged
                    .Where(x => x.EventType == Data.Common.EntityEventType.Created)
                    .Select(x => x.Entity)
                    .Where(x => !(x is IDeliveryOrder))
                    .Subscribe(x => Operations.AddNotificationMessage(string.Format(OrderInfoPluginLocalResources.CreatedOrder, x.Number), nameof(OrderInfoPlugin))),

                Notifications
                    .DeliveryOrderChanged
                    .Where(x => x.EventType == Data.Common.EntityEventType.Created)
                    .Select(x => x.Entity)
                    .Subscribe(x => Operations.AddNotificationMessage(string.Format(OrderInfoPluginLocalResources.CreatedDeliveryOrder, x.Number), nameof(OrderInfoPlugin))),

                Notifications.CurrentCultureChanged.Subscribe(x => {
                        CultureInfo.CurrentCulture = x.culture;
                        CultureInfo.CurrentUICulture = x.uiCulture;
                        CultureInfo.DefaultThreadCurrentCulture = x.culture;
                        CultureInfo.DefaultThreadCurrentUICulture = x.uiCulture;
                }),
            };

            Operations.AddNotificationMessage(string.Format(OrderInfoPluginLocalResources.PluginStarted, nameof(OrderInfoPlugin)), nameof(OrderInfoPlugin));
            

            Log.Info("OrderInfoPlugin started");
        }

        public IDisposable AddButtonNumberOfPositions() {
            return Operations
                    .AddButtonToOrderEditScreen(OrderInfoPluginLocalResources.NumberOfPositionsInOrderButton,
                        x => x.vm.ShowOkPopup(OrderInfoPluginLocalResources.NumberOfPositionsInOrderCaption,
                            string.Format(OrderInfoPluginLocalResources.NumberOfPositionsInOrderMessage, x.order.Items.Count)));
        }

        public void Dispose() {
            subscriptions.Dispose();
        }
    }
}
