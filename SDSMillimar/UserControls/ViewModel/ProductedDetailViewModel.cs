using HandyControl.Controls;
using SDSMillimar.Common;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDSMillimar.UserControls.ViewModel
{
    public class ProductedDetailViewModel : BaseViewModel
    {
        private ProductRepository repo = null;
        private Product productedInfo;

        public Product ProductedInfo
        {
            get { return productedInfo; }
            set
            {
                productedInfo = value;
                if (productedInfo.Id == 0)
                    TitleText = "新增零件";
                else
                    TitleText = $"编辑[{productedInfo.ProductName}]";
                OnPropertyChanged();
            }
        }

        private string titleText;

        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; OnPropertyChanged(); }
        }




        private bool isAdd = true;

        public bool IsAdd
        {
            get { return isAdd; }
            set { isAdd = value; OnPropertyChanged(); }
        }


        public ICommand LoadedCommand { get; set; }
        public ICommand ClosedCommand { get; set; }
        public ICommand SubmitCommand { get; set; }
        public ProductedDetailViewModel(Product productedInfo)
        {
            IsAdd = productedInfo == null;
            ProductedInfo = productedInfo ?? new Product();
            repo = new ProductRepository();
            SubmitCommand = new RelayCommand(async () => await SubmitAsync());
            ClosedCommand = new RelayCommand(Closed);
        }

        private async Task SubmitAsync()
        {
            try
            {
                var result = IsAdd ? await repo.AddAsync(ProductedInfo) : await repo.UpdateAsync(ProductedInfo);
                if (result > 0)
                {
                    Growl.Success("提交成功");
                    Closed();
                }
            }
            catch (Exception ex)
            {
                Growl.Error($"提交失败:\n{ex.Message}");
            }
            finally
            {
                Messenger.Send("RefreshData", true);
            }
        }



        private void Closed()
        {
            Dialog.Close(DialogTokens.ProductDetail);
        }
    }
}
