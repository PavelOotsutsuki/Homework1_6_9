using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Homework1_6_9
{
    class Program
    {
        static void Main(string[] args)
        {
            int countCustomers = 4;
            Shop shop = new Shop(countCustomers);

            shop.ReceptionCustomersAtCheckout();
        }
    }

    class Product
    {
        public Product (string title)
        {
            Title = title;
        }

        public string Title { get; protected set; }

        public virtual void ShowInfo()
        {
            Console.Write(Title + ": ");
        }
    }

    class SaleItem: Product 
    {
        public SaleItem(string title, float price) : base(title)
        {
            Price = price;
        }

        public float Price { get; private set; }

        public override void ShowInfo()
        {
            base.ShowInfo();
            Console.Write(Price + " руб/шт");
        }
    }

    class Customer
    {
        private float _money;
        private Random _random;
        private RackProduct _products;

        public Customer(float money, int number)
        {
            Number = number;
            Basket = new Rack();
            _money = money;
            _random = new Random();
            _products = new RackProduct();
        }

        public int Number { get; private set; }
        public Rack Basket { get; private set; }

        public void ShowAllSaleItems()
        {
            Console.WriteLine("Мои товары:");
            Basket.ShowAllSaleItems();
        }

        private float SumCostSaleItems()
        { 
            return Basket.SumCostSaleItems();
        }

        public bool CanBuySaleItems()
        {
            return SumCostSaleItems() <= _money;
        }

        public bool TryFindReturnableSaleItem(out CellSaleItem saleItem)
        {
            int countSaleItems=Basket.SumCountSaleItems();

            if (countSaleItems > 0)
            {
                int returnableSaleItemNumber = _random.Next(1, countSaleItems+1);
                CellSaleItem returnableSaleItem = Basket.FindSaleItemByNumber(returnableSaleItemNumber);
                saleItem = returnableSaleItem;
                return true;
            }

            Console.WriteLine("Корзина пуста");
            saleItem = null;
            return false;
        }

        public void BuySaleItems()
        {
            _money -= SumCostSaleItems();

            List<CellProduct> cellProducts = Basket.HandOverSaleItems();

            foreach (var cell in cellProducts)
            {
                _products.AddSaleItem(cell);
            }
        }

        public void ShowInfo()
        {
            Console.WriteLine("Осталось " + _money + " рублей. Купленные продукты:");
            Console.WriteLine();
            _products.ShowAllProducts();
        }

        public void ShowMoneyInfo()
        {
            Console.WriteLine("Есть: " + _money + " рублей. Необходимо " + SumCostSaleItems() + " рублей");
        }
    }

    class Shop
    {
        private Queue<Customer> _customers = new Queue<Customer>();
        private Rack _allSaleItems = new Rack();
        private Random _random = new Random(); 

        public Shop (int countCustomers)
        {
            FillSaleItemsList();
            FillCustomersData(countCustomers);
        }

        private void FillCustomersData(int countCustomers)
        {
            int minMoney = 5000;
            int maxMoney = 120000;
            int money;

            for (int i=0;i<countCustomers;i++)
            {
                money = _random.Next(minMoney, maxMoney+1);
                _customers.Enqueue(new Customer(money, i+1));
            }

            foreach (var customer in _customers)
            {
                AddSaleItems(customer);
            }
        }

        public void ReceptionCustomersAtCheckout()
        {
            while (_customers.Count>0)
            {
                Console.Clear();
                Customer customer = _customers.Dequeue();
                Console.WriteLine("Клиент " + customer.Number);
                Console.WriteLine();
                BuySaleItems(customer);
                Console.ReadKey();
            }
        }

        private void BuySaleItems(Customer customer)
        {
            while(customer.CanBuySaleItems()==false)
            {
                ReturnRandomSaleItem(customer);
                Console.ReadKey();
            }

            customer.BuySaleItems();
            customer.ShowInfo();
            Console.WriteLine("Клиент обслужен!");
        }

        private void ReturnRandomSaleItem(Customer customer)
        {
            int countReturnableSaleItems = 1;

            if (customer.TryFindReturnableSaleItem(out CellSaleItem saleItem))
            {
                Console.Write("Вы убрали " + saleItem.SaleItem.Title + " в размере " + countReturnableSaleItems + "шт. ");
                HandOverSaleItem(saleItem, countReturnableSaleItems, customer.Basket, _allSaleItems);
                customer.ShowMoneyInfo();
            }
        }

        private void FillSaleItemsList()
        {
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Война и мир", 1600), 300));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Бокалы для вина", 1200), 23));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Зубная щетка", 140.4f), 46));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Шампунь", 244.5f), 256));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Кровать", 16000), 3));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Пылесос", 49000), 10));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Галстук", 760), 30));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Кофе", 456), 456));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Чай черный", 300), 300));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Чай зеленый", 250), 350));
            _allSaleItems.AddSaleItem(new CellSaleItem(new SaleItem("Кровь вампира", 100000.1f), 1));

            _allSaleItems.RemoveVoidSaleItems();
        }

        private void AddSaleItems(Customer customer)
        {
            const string AddSaleItemCommand = "1";
            const string ReturnSaleItemCommand = "2";
            const string EndAddSaleItemsCommand = "3";

            bool endAddSaleItems = false;

            while (endAddSaleItems==false)
            {
                Console.Clear();
                Console.WriteLine("Клиент " + customer.Number + ":");
                Console.WriteLine(AddSaleItemCommand + ". Добавить в корзину товар");
                Console.WriteLine(ReturnSaleItemCommand + ". Вернуть товар на полку");
                Console.WriteLine(EndAddSaleItemsCommand + ". Пойти на кассу");
                Console.Write("\nВведите номер команды: ");
                string userInput = Console.ReadLine();

                switch (userInput)
                {
                    case AddSaleItemCommand:
                        AddSaleItem(customer);
                        break;

                    case ReturnSaleItemCommand:
                        ReturnSaleItem(customer);
                        break;

                    case EndAddSaleItemsCommand:
                        endAddSaleItems = true;
                        break;

                    default:
                        Console.WriteLine("Введена неверная команда");
                        break;
                }

                Console.ReadKey();
            }
        }

        private void ReturnSaleItem(Customer customer)
        {
            Console.Clear();
            customer.ShowAllSaleItems();
            Console.Write("\nВведите имя продукта, который хотите вернуть на полку: ");

            HandOverSaleItems(customer.Basket, _allSaleItems);
        }

        private void HandOverSaleItems(Rack givingRack, Rack gettingRack)
        {
            if (givingRack.TryGetCell(Console.ReadLine(), out CellSaleItem saleItemCell))
            {
                Console.WriteLine("Введите кол-во данного товара: ");

                if (int.TryParse(Console.ReadLine(), out int wishingQuantity))
                {
                    if (saleItemCell.Quantity - wishingQuantity >= 0)
                    {
                        HandOverSaleItem(saleItemCell, wishingQuantity, givingRack, gettingRack);
                    }
                    else
                    {
                        Console.WriteLine("Столько товара нет!");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод кол-ва товара");
                }

            }
            else
            {
                Console.WriteLine("Такого продукта нет!");
            }
        }

        private void HandOverSaleItem(CellSaleItem saleItemCell, int wishingQuantity, Rack givingRack, Rack gettingRack)
        {
            saleItemCell.HandOverSaleItem(wishingQuantity);
            CellSaleItem addedSaleItem = new CellSaleItem(saleItemCell.SaleItem, wishingQuantity);
            gettingRack.AddSaleItem(addedSaleItem);
            givingRack.RemoveVoidSaleItems();
        }

        private void AddSaleItem(Customer customer)
        {
            Console.Clear();
            customer.ShowAllSaleItems();
            Console.WriteLine("Товары доступные в магазине:");
            Console.WriteLine();
            _allSaleItems.ShowAllSaleItems();
            Console.Write("\nВведите имя продукта, который хотите положить в корзину: ");

            HandOverSaleItems(_allSaleItems, customer.Basket);
        }
    }

    class Rack
    {
        private List<CellSaleItem> _saleItems;

        public Rack ()
        {
            _saleItems = new List<CellSaleItem>();
        }

        public List<CellProduct> HandOverSaleItems()
        {
            List<CellProduct> cellProducts = new List<CellProduct>();

            foreach (var saleItem in _saleItems)
            {
                cellProducts.Add(new CellProduct(new Product(saleItem.SaleItem.Title), saleItem.Quantity));
                saleItem.HandOverSaleItem(saleItem.Quantity);
            }

            RemoveVoidSaleItems();
            return cellProducts;
        }

        public int SumCountSaleItems()
        {
            int sumCount = 0;

            foreach (var cell in _saleItems)
            {
                sumCount += cell.Quantity;
            }

            return sumCount;
        }

        public CellSaleItem FindSaleItemByNumber(int number)
        {
            int sumCount = 0;

            foreach (var cell in _saleItems)
            {
                sumCount += cell.Quantity;

                if (number <= sumCount)
                {
                    return cell;
                }
            }

            Console.WriteLine("Ошибка кол-ва товаров");
            Console.ReadKey();
            return null;
        }

        public void AddSaleItem(CellSaleItem addedSaleItem)
        {
            bool isFind = false;

            for (int i = 0; i < _saleItems.Count && isFind == false; i++)
            {
                if (_saleItems[i].SaleItem.Title == addedSaleItem.SaleItem.Title)
                {
                    _saleItems[i].AddSaleItem(addedSaleItem.Quantity);
                    isFind = true;
                }
            }

            if (isFind == false)
            {
                _saleItems.Add(addedSaleItem);
            }
        }

        public void RemoveVoidSaleItems()
        {
            int countCells= _saleItems.Count;

            for (int i=0; i< countCells; i++)
            {
                if (_saleItems[i].Quantity <= 0)
                {
                    _saleItems.Remove(_saleItems[i]);
                    countCells--;
                }
            }
        }

        public float SumCostSaleItems()
        {
            float sumCost = 0;

            foreach (var cell in _saleItems)
            {
                sumCost += cell.Quantity * cell.SaleItem.Price;
            }

            return sumCost;
        }

        public void ShowAllSaleItems()
        {
            if (_saleItems.Count > 0)
            {
                foreach (var cell in _saleItems)
                {
                    cell.ShowInfo();
                }
            }
            else
            {
                Console.WriteLine("Пусто");
            }
        }

        public bool TryGetCell(string title, out CellSaleItem saleItemCell)
        {
            foreach (var cell in _saleItems)
            {
                if (cell.SaleItem.Title == title)
                {
                    saleItemCell = cell;
                    return true;
                }
            }

            saleItemCell = null;
            return false;
        }
    }

    class CellSaleItem
    {
        public CellSaleItem(SaleItem saleItem, int quantity)
        {
            SaleItem = saleItem;
            Quantity = quantity;
        }

        public SaleItem SaleItem { get; private set; }
        public int Quantity { get; private set; }

        public void HandOverSaleItem(int quantity)
        {
            Quantity -= quantity;
        }

        public void AddSaleItem(int quantity)
        {
            Quantity += quantity;
        }

        public void ShowInfo()
        {
            SaleItem.ShowInfo();
            Console.WriteLine(", остаток: " + Quantity + " шт");
        }
    }

    class CellProduct
    {
        public CellProduct(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public Product Product { get; private set; }
        public int Quantity { get; private set; }

        public void ShowInfo()
        {
            Product.ShowInfo();
            Console.WriteLine("кол-во: " + Quantity + " шт");
        }

        public void AddProduct(int quantity)
        {
            Quantity += quantity;
        }
    }

    class RackProduct
    {
        private List<CellProduct> _products;

        public RackProduct()
        {
            _products = new List<CellProduct>();
        }

        public void AddSaleItem(CellProduct addedProduct)
        {
            bool isFind = false;

            for (int i = 0; i < _products.Count && isFind == false; i++)
            {
                if (_products[i].Product.Title == addedProduct.Product.Title)
                {
                    _products[i].AddProduct(addedProduct.Quantity);
                    isFind = true;
                }
            }

            if (isFind == false)
            {
                _products.Add(addedProduct);
            }
        }

        public void ShowAllProducts()
        {
            if (_products.Count > 0)
            {
                foreach (var cell in _products)
                {
                    cell.ShowInfo();
                }
            }
            else
            {
                Console.WriteLine("Пусто");
            }
        }
    }
}
