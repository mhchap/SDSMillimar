using HandyControl.Controls;
using SDSMillimar.Common;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using SDSMillimar.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SDSMillimar.UserControls.ViewModel
{
    public class TechnologyDetailViewModel : BaseViewModel
    {
        private TechnologyRepository repo = null;
        private ProductRepository productRepository = null;
        public Action RequestClose { get; set; }
        private string selectProductedId;

        public string SelectProductedId
        {
            get { return selectProductedId; }
            set
            {
                selectProductedId = value;
                OnPropertyChanged();
            }
        }


        private Technology technologyInfo;

        public Technology TechnologyInfo
        {
            get { return technologyInfo; }
            set
            {
                technologyInfo = value;
                if (technologyInfo.Id == 0)
                    TitleText = "新增工艺";
                else
                    TitleText = $"编辑[{technologyInfo.TechnologyName}]";
                OnPropertyChanged();
            }
        }

        private List<TechnologyType> technologyTypes = new List<TechnologyType> { new TechnologyType { Id = 1, Name = "测量" }, new TechnologyType { Id = 2, Name = "校准" } };

        public List<TechnologyType> TechnologyTypes
        {
            get { return technologyTypes; }
            set { technologyTypes = value; OnPropertyChanged(); }
        }


        private List<OilGroove> oilGrooveDatas = new List<OilGroove> { new OilGroove { Id = 0, Name = "不开启" }, new OilGroove { Id = 1, Name = "开启" } };

        public List<OilGroove> OilGrooveDatas
        {
            get { return oilGrooveDatas; }
            set { oilGrooveDatas = value; OnPropertyChanged(); }
        }

        //private OilGroove selectedOilGroove;

        //public OilGroove SelectedOilGroove
        //{
        //    get { return selectedOilGroove; }
        //    set
        //    {
        //        selectedOilGroove = value;
        //        OnPropertyChanged();
        //    }
        //}


        private TechnologyType selectedTechnologyType;

        public TechnologyType SelectedTechnologyType
        {
            get { return selectedTechnologyType; }
            set
            {
                selectedTechnologyType = value;
                OnPropertyChanged();
                if (IsAdd)
                {
                    TechnologyInfo.TechnologyCode = Guid.NewGuid().ToString();
                    TechnologyInfo.TechnologyName = $"{SelectedProduct?.ProductName}-{selectedTechnologyType?.Name}";
                    OnPropertyChanged(nameof(TechnologyInfo));
                }
            }
        }


        private List<Product> products;

        public List<Product> Products
        {
            get { return products; }
            set { products = value; OnPropertyChanged(); }
        }

        private Product selectedProduct;

        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                selectedProduct = value;
                TechnologyInfo.ProductName = selectedProduct?.ProductName;
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

        public ICommand ClosedCommand { get; set; }
        public ICommand SubmitCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public ICommand SelectProbeCommand { get; set; }
        public TechnologyDetailViewModel(Technology info)
        {
            IsAdd = info == null;
            TechnologyInfo = info ?? new Technology();
            repo = new TechnologyRepository();
            productRepository = new ProductRepository();
            SubmitCommand = new RelayCommand(async () => await SubmitAsync());
            ClosedCommand = new RelayCommand(Closed);
            LoadedCommand = new RelayCommand(Loaded);
            SelectProbeCommand = new RelayCommand(SelectProbe);
        }

        private void SelectProbe()
        {
            var probeView = new ProbeView();
            probeView.Owner = Application.Current.MainWindow;
            probeView.ShowDialog();

        }

        private void Loaded()
        {
            Task.Run(async () =>
            {
                var result = await productRepository.GetPageAsync(1, 1000);
                Products = result.Items;

            });
        }

        private async Task SubmitAsync()
        {
            try
            {
                TechnologyInfo.Validate(out string error);
                if (!string.IsNullOrEmpty(error))
                {
                    Growl.Warning(error);
                    return;
                }

                var result = IsAdd ? await repo.AddAsync(TechnologyInfo) : await repo.UpdateAsync(TechnologyInfo);
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
            //Dialog.Close(DialogTokens.TechnologyView);
            RequestClose?.Invoke();
        }
    }
}
