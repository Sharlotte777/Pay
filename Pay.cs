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

            IPaymentSystem loggedPaymentSystem1 = new PaymentSystem1();
            IPaymentSystem loggedPaymentSystem2 = new PaymentSystem2();
            IPaymentSystem loggedPaymentSystem3 = new PaymentSystem3("secret_key");

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

    public interface IPaymentSystem
    {
        string GetPayingLink(Order order);
    }

    public interface IHashAlgorithm
    {
        string ComputeHash(string input);
    }

    public class MD5HashAlgorithm : IHashAlgorithm
    {
        public string ComputeHash(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public class SHA1HashAlgorithm : IHashAlgorithm
    {
        public string ComputeHash(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public class PaymentSystem1 : IPaymentSystem
    {
        private readonly IHashAlgorithm _hashAlgorithm;

        public PaymentSystem1()
        {
            _hashAlgorithm = new MD5HashAlgorithm();
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashAlgorithm.ComputeHash(order.Id.ToString());
            return $"https://pay.system1.ru/order?amount={order.Amount}RUB&hash={hash}";
        }
    }

    public class PaymentSystem2 : IPaymentSystem
    {
        private readonly IHashAlgorithm _hashAlgorithm;

        public PaymentSystem2()
        {
            _hashAlgorithm = new MD5HashAlgorithm();
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashAlgorithm.ComputeHash(order.Id.ToString() + order.Amount.ToString());
            return $"https://order.system2.ru/pay?hash={hash}";
        }
    }

    public class PaymentSystem3 : IPaymentSystem
    {
        private readonly string _secretKey;
        private readonly IHashAlgorithm _hashAlgorithm;

        public PaymentSystem3(string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("Secret key cannot be null or whitespace.", nameof(secretKey));

            _secretKey = secretKey;
            _hashAlgorithm = new SHA1HashAlgorithm();
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashAlgorithm.ComputeHash(order.Amount.ToString() + order.Id.ToString() + _secretKey);
            return $"https://system3.com/pay?amount={order.Amount}&currency=RUB&hash={hash}";
        }
    }
}
