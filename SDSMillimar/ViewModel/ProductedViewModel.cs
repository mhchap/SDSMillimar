using HandyControl.Controls;
using HandyControl.Data;
using SDSMillimar.Common;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.UserControls;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    public class ProductedViewModel : BaseViewModel
    {

        private readonly ProductRepository repo = null;

        private bool isLoading = true;

        public bool IsLoading
        {
            get { return isLoading; }
            set { isLoading = value; OnPropertyChanged(); }
        }

        private string keyword;

        public string Keyword
        {
            get { return keyword; }
            set { keyword = value; OnPropertyChanged(); }
        }


        private int pageIndex = 1;
        public int PageIndex
        {
            get => pageIndex;
            set { pageIndex = value; OnPropertyChanged(); }
        }

        private int maxPageCount = 1;

        public int MaxPageCount
        {
            get { return maxPageCount; }
            set { maxPageCount = value; OnPropertyChanged(); }
        }

        private List<Product> datas = new List<Product>();

        public List<Product> Datas
        {
            get { return datas; }
            set { datas = value; OnPropertyChanged(); }
        }

        public ICommand LoadedCommand { get; set; }
        public ICommand AddProductedCommand { get; set; }
        public ICommand EditProductedCommand { get; set; }
        public ICommand DeleteProductedCommand { get; set; }
        public ICommand PageUpdatedCommand { get; set; }
        public ICommand QueryCommand { get; set; }
        public ICommand ResetKeywordCommand { get; set; }


        private Product selectedData;
        public Product SelectedData
        {
            get => selectedData;
            set
            {
                selectedData = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BtnControlCan));
            }
        }

        public bool BtnControlCan
        {
            get
            {
                return SelectedData != null;
            }
        }


        public ProductedViewModel()
        {
            LoadedCommand = new RelayCommand(Loaded);
            QueryCommand = new RelayCommand(Query);
            ResetKeywordCommand = new RelayCommand(ResetKeyword);
            AddProductedCommand = new RelayCommand(AddProducted);
            EditProductedCommand = new RelayCommand(EditProducted);
            DeleteProductedCommand = new RelayCommand(DeleteProducted);
            PageUpdatedCommand = new RelayCommand<object>((args) =>
            {
                if (args is FunctionEventArgs<int> e)
                {
                    PageIndex = e.Info;
                    Loaded();
                }
            });
            Messenger.Register<bool>("RefreshData", RefreshData);
            repo = new ProductRepository();
        }

        private void ResetKeyword()
        {
            Keyword = string.Empty;
            Query();
        }

        private void RefreshData(bool isRefresh)
        {

            if (isRefresh)
                Loaded();
        }

        private void AddProducted()
        {
            var view = new ProductedDetailControl(null);
            Dialog.Show(view, DialogTokens.ProductDetail);
        }

        private void DeleteProducted()
        {
            Task.Run(async () =>
            {
                try
                {
                    var result = await repo.SoftDeleteAsync(SelectedData.Id);
                    if (result > 0)
                    {
                        Growl.Success("删除成功");
                        Loaded();
                    }
                }
                catch (Exception ex)
                {
                    Growl.Error($"删除失败:{ex.Message}");
                }

            });
        }

        private void EditProducted()
        {
            var view = new ProductedDetailControl(SelectedData);
            Dialog.Show(view, DialogTokens.ProductDetail);
        }

        private void Loaded()
        {
            Query();
        }

        private void Query()
        {
            _ = Task.Run(async () =>
            {
                IsLoading = true;
                var result = string.IsNullOrEmpty(Keyword) ? await repo.GetPageAsync(PageIndex, 10) : await repo.GetPageAsync(PageIndex, 10, Keyword);
                IsLoading = false;
                // 总页数 = 总条数 / 每页条数 向上取整
                int totalPages = (int)Math.Ceiling(result.TotalCount / 10.0);
                MaxPageCount = totalPages;
                Datas = result.Items;
            });

        }
    }
}
