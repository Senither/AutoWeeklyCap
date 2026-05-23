using Microsoft.VisualBasic;

namespace AutoWeeklyCap.Enums;

public enum DeliverooStuckAction
{
    StopDeliveroo,
    HandInMateriaItem
}

public static class DeliverooStuckActionExtensions
{
    extension(DeliverooStuckAction action)
    {
        public string GetName()
        {
            return action switch
            {
                DeliverooStuckAction.StopDeliveroo => "Stop Deliveroo",
                DeliverooStuckAction.HandInMateriaItem => "Hand in item with materia",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
        }

        public string GetTooltip()
        {
            return (action switch
            {
                DeliverooStuckAction.StopDeliveroo => Strings.Join([
                    "If the runner detects that Deliveroo is stuck on an item with materia, it will",
                    "quickly stop Deliveroo and resume the runner to prevent getting stuck.",
                ], "\n"),
                DeliverooStuckAction.HandInMateriaItem => Strings.Join([
                    "If the runner detects that Deliveroo is stuck on an item with materia, it will",
                    "hand in the item to continue the Deliveroo action, and then resume the runner",
                    "after it's done. Doing this will destroy the materia, and might hand in items",
                    "you don't have associated with any gear set",
                ], "\n"),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            })!;
        }
    }
}
