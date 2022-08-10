using System;

namespace WebApiRabbitMQ.Domain
{
    public class Order
    {
        public int OrderNumber { get; set; }
        public string ItemName{ get; set; }
        public float Price { get; set; }
        public DateTime Last_Update { get; set; }

        public Order(int orderNumber)
        {
            OrderNumber = orderNumber;
            Last_Update = DateTime.Now;
        }

        public void Update(string itemName)
        {
            ItemName = itemName;
            Last_Update = DateTime.Now;
        }
    }
}
