using AvaloniaApplication19.Command;
using AvaloniaApplication19.Data;
using AvaloniaApplication19.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaApplication19.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        
        private readonly Dictionary<string, List<string>> _errors = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public ObservableCollection<Product> Products { get; } = new();
        public ICommand LoadProductsCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand ClearInputCommand { get; }



        public MainViewModel()
        {
            LoadProductsCommand = new RelayCommand(_ => LoadProducts());
            AddProductCommand = new RelayCommand(_ => AddProduct());
            AddProductCommand = new RelayCommand(_ => UpdateProductAsync());
            ClearInputCommand = new RelayCommand(_ => DeleteProductAsync());
            ClearInputCommand = new RelayCommand(_ => ClearInputAsync());


            InitializeAsync();
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

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                
                if (value != null)
                {
                    Name = value.Name;
                    Category = value.Category;
                    PriceText = value.Price.ToString();
                }
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
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
        private async Task InitializeAsync()
        {
            await CreateDatabaseAsync();
            await LoadProductsAsync();
        }
        private async Task CreateDatabaseAsync()
        {
            using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();
            bool hasProduct = await db.Products.AnyAsync();
            if (!hasProduct)
            {
                db.Products.AddRange(
                    new Product { Name = "ноутбук", Category = "Компьютеры и ноутбуки", Price = 85000 },
                    new Product { Name = "ноутбук", Category = "Компьютеры и ноутбуки", Price = 85000 },
                    new Product { Name = "ноутбук", Category = "Компьютеры и ноутбуки", Price = 85000 }
                    );
                await db.SaveChangesAsync();
            }
        }
        private async Task LoadProductsAsync()
        {
            IsLoading = true;
            Info = "загрузка...";

            Products.Clear();
            await Task.Delay(500);
            using var db = new AppDbContext();


            var products = await db.Products
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .ToListAsync();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            Info = $"Загружено: {Products.Count}";
            IsLoading = false;
        }
        
            
      
        private async Task AddProduct()
        {
            ValidateAll();

            if (HasErrors)
            {
                Info = "Исправьте ошибки перед добавлением";
                return;
            }
            double price = double.Parse(PriceText);

            IsLoading = true;
            Info = ""

            using var db = new AppDbContext();
            var product = new Product
            {
                Name = Name,
                Category = Category,
                Price = price
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            await ClearInputAsync();
            await LoadProductsAsync();

            Info = "Товар добавлен";
            IsLoading = false;

            

        }
        private async Task UpdateProductAsync()
        {
            if(SelectedProduct == null)
            {
                Info = "Исправьте ошибкии перед добавлением";
                return;
            }
            ValidateAll();

            if (HasErrors)
            {
                Info = "Исправьте ошибкии перед добавлением";
                return;
            }
            double price = double.Parse (PriceText);

            IsLoading = true;
            Info = "Обновление";

            using var db = new AppDbContext();
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == SelectedProduct.Id);
            if (product == null)
            {
                Info = "Товар не найден";
                IsLoading = false;
                return;
            }
            product.Name = Name;
            product.Category = Category;
            product.Price = price;
            await db.SaveChangesAsync();

            await ClearInputAsync();
            await LoadProductsAsync();

            Info = "Товар обновлён";
            IsLoading = false;
        }
         
        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
            {
                Info = "Выберите товар для удаления";
                return;
            }
            

            

            IsLoading = true;
            Info = "Удаление";

            using var db = new AppDbContext();
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == SelectedProduct.Id);
            if (product == null)
            {
                Info = "Товар не найден";
                IsLoading = false;
                return;
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();

            Info = "Товар обновлён";
            IsLoading = false;
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

    
    

