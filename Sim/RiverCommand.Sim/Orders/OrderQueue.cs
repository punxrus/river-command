using System;
using System.Collections.Generic;

namespace RiverCommand.Sim
{
    /// <summary>
    /// Shift-click queue. Issue(append:false) replaces the queue; append:true adds to the end.
    /// Waypoint nodes can be dragged (MoveWaypoint) or deleted (RemoveAt).
    /// </summary>
    public sealed class OrderQueue
    {
        private readonly List<Order> _orders = new List<Order>();

        public IReadOnlyList<Order> Items => _orders;
        public Order Current => _orders.Count > 0 ? _orders[0] : null;
        public int Count => _orders.Count;

        public void Issue(Order order, bool append)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (!append) _orders.Clear();
            _orders.Add(order);
        }

        public void IssueAll(IEnumerable<Order> orders, bool append)
        {
            if (!append) _orders.Clear();
            _orders.AddRange(orders);
        }

        public void Clear() => _orders.Clear();

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _orders.Count) _orders.RemoveAt(index);
        }

        /// <summary>Drag edit: move the waypoint of a queued move order. Returns false if not draggable.</summary>
        public bool MoveWaypoint(int index, Vec2 newPosition)
        {
            if (index < 0 || index >= _orders.Count) return false;
            if (_orders[index] is MoveOrder move) { move.Target = newPosition; return true; }
            return false;
        }

        /// <summary>Total path length (m) from a start position through all queued waypoints.</summary>
        public double PathLengthFrom(Vec2 start)
        {
            double total = 0;
            var at = start;
            foreach (var o in _orders)
            {
                if (o.Waypoint is Vec2 wp) { total += Vec2.Distance(at, wp); at = wp; }
            }
            return total;
        }

        internal void Tick(Unit unit, SimWorld world, double dt)
        {
            if (unit.Status == UnitStatus.OutOfFuel) return;
            var current = Current;
            if (current == null)
            {
                unit.Velocity = Vec2.Zero;
                if (unit.Status != UnitStatus.Moored) unit.Status = UnitStatus.Holding;
                return;
            }
            current.Execute(unit, world, dt);
            if (current.IsComplete) _orders.RemoveAt(0);
        }
    }
}
