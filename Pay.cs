using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;

public class Pay : MonoBehaviour
{
    class Program
    {
        static void Main(string[] args)
        {
            Order order = new Order(1, 12000);

            IPaymentSystem loggedPaymentSystem1 = new LoggingDecorator(new PaymentSystem1());
            IPaymentSystem loggedPaymentSystem2 = new LoggingDecorator(new PaymentSystem2());
            IPaymentSystem loggedPaymentSystem3 = new LoggingDecorator(new PaymentSystem3("secret_key"));

            Console.WriteLine(loggedPaymentSystem1.GetPayingLink(order));
            Console.WriteLine(loggedPaymentSystem2.GetPayingLink(order));
            Console.WriteLine(loggedPaymentSystem3.GetPayingLink(order));
        }
    }

    public class Order
    {
        public readonly int Id;
        public readonly int Amount;

        public Order(int id, int amount) => (Id, Amount) = (id, amount);
    }

    public abstract class PaymentSystemBase : IPaymentSystem
    {
        public abstract string GetPayingLink(Order order);

        protected string CalculateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        protected string CalculateSHA1Hash(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public class PaymentSystem1 : PaymentSystemBase
    {
        public override string GetPayingLink(Order order)
        {
            string hash = CalculateMD5Hash(order.Id.ToString());
            return $"https://pay.system1.ru/order?amount={order.Amount}RUB&hash={hash}";
        }
    }

    public class PaymentSystem2 : PaymentSystemBase
    {
        public override string GetPayingLink(Order order)
        {
            string hash = CalculateMD5Hash(order.Id.ToString() + order.Amount.ToString());
            return $"https://order.system2.ru/pay?hash={hash}";
        }
    }

    public class PaymentSystem3 : PaymentSystemBase
    {
        private readonly string _secretKey;

        public PaymentSystem3(string secretKey)
        {
            _secretKey = secretKey;
        }

        public override string GetPayingLink(Order order)
        {
            string hash = CalculateSHA1Hash(order.Amount.ToString() + order.Id.ToString() + _secretKey);
            return $"https://system3.com/pay?amount={order.Amount}&currency=RUB&hash={hash}";
        }
    }

    public interface IPaymentSystem
    {
        string GetPayingLink(Order order);
    }

    public class LoggingDecorator : IPaymentSystem
    {
        private readonly IPaymentSystem _paymentSystem;

        public LoggingDecorator(IPaymentSystem paymentSystem)
        {
            _paymentSystem = paymentSystem;
        }

        public string GetPayingLink(Order order)
        {
            Console.WriteLine($"Создание ссылки для заказа ID: {order.Id}, сумма: {order.Amount}");

            return _paymentSystem.GetPayingLink(order);
        }
    }
}
