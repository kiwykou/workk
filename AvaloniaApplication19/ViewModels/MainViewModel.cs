using AvaloniaApplication19.Command;
using AvaloniaApplication19.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace AvaloniaApplication19.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private const string ConnectionString = "Data Source=app.db";
        private readonly Dictionary<string, List<string>> _errors = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public ObservableCollection<Product> Products { get; } = new();
        public ICommand LoadProductsCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand ClearInputCommand { get; }
        public ICommand CheckLengthCommand { get; }
        public ICommand CheckPriceCommand {  get; }

        public MainViewModel()
        {
            LoadProductsCommand = new RelayCommand(_ => LoadProducts());
            AddProductCommand = new RelayCommand(_ => AddProduct());
            ClearInputCommand = new RelayCommand(_ => ClearInput());
           

            LoadProducts();
        }
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                ValidateName();
            }
        }
        private string _category = "";
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
                ValidateCategory();
            }
        }
        private string _priceText = "";
        public string PriceText
        {
            get => _priceText;
            set
            {
                _priceText = value;
                OnPropertyChanged();
                ValidatePrice();
            }
        }
        private string _info = "";
        public string Info
        {
            get => _info;
            set
            {
                _info = value;
                OnPropertyChanged();

            }
        }
        private string _description= "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
                ValidateDescription();
            }
        }
        public bool HasErrors => _errors.Count > 0;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null)
            {
                return Array.Empty<string>();
            }
            if (_errors.TryGetValue(propertyName, out var errors))
            {
                return errors;
            }
            return Array.Empty<string>();
        }
        private void LoadProducts()
        {
            Products.Clear();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT Id, Name, Category, Price
                FROM Products
                ORDER BY Id;
                """;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Price = reader.GetDouble(3)
                  
                });

            }
            Info = $"Загружено товаров {Products.Count}";
        }
        private void AddProduct()
        {
            ValidateAll();
            if (HasErrors)
            {
                Info = "Исправьте ошибки перед добавлением";
                return;
            }
            double price = double.Parse(PriceText);

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                """
                INSERT  INTO Products (Name, Category, Price, Description)
                VALUES ($name, $category, $price, $description);
                """;

            command.Parameters.AddWithValue("$name", Name);
            command.Parameters.AddWithValue("$category", Category);
            command.Parameters.AddWithValue("$price", price);
            command.Parameters.AddWithValue("$description", Description);

            command.ExecuteNonQuery();

            Info = "Товар добавлен";

            ClearInput();
            LoadProducts();

        }

        private void ClearInput()
        {
            Name = "";
            Category = "";
            PriceText = "";
            Description = "";

            ClearErrors(nameof(Name));
            ClearErrors(nameof(Category));
            ClearErrors(nameof(PriceText));
            ClearErrors(nameof(Description));

            Info = "Поля очищены";
        }
        private void ValidateAll()
        {
            ValidateName();
            ValidateCategory();
            ValidatePrice();
            ValidateDescription();
        }
        private void ValidateName()
        {
            ClearErrors(nameof(Name));
            if (string.IsNullOrEmpty(Name))
            {
                AddError(nameof(Name), "Название товара обязательно");
            }
            if (Name.Length < 3)
            {
                AddError(nameof(Name), "Минимум 3 символа");
            }
        }
        private void ValidateCategory()
        {
            ClearErrors(nameof(Category));
            if (string.IsNullOrEmpty(Category))
            {
                AddError(nameof(Category), "Категория обязательна");
            }
        }
        private void ValidatePrice()
        {
            ClearErrors(nameof(PriceText));
            if (string.IsNullOrWhiteSpace(PriceText))
            {
                AddError(nameof(PriceText), "Цена обязательна");
                return;
            }
            if (!double.TryParse(PriceText, out double price))
            {
                AddError(nameof(PriceText), "Цена должна быть числом");
                return;
            }
            if (price > 1000000)
            {
                AddError(nameof(PriceText), "Цена не должна быть больше 1000000");
            }
        }
        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                _errors[propertyName] = new List<string>();
            }
            _errors[propertyName].Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(propertyName));
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                OnPropertyChanged(nameof(HasErrors));
            }
        }
        private void ValidateDescription()
        {
            ClearErrors(nameof(Description));
           
           

            if (Description.Length < 10)
            {
                AddError(nameof(Description), "Описание необязательное, но если заполнено - минимум 10 символов");
            }

        }


        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

    
    

