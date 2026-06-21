using SDSMillimar.Common;
using SDSMillimar.Dtos;
using SDSMillimar.Models;
using SDSMillimar.Services;
using SDSMillimar.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    public class ModelSelectionViewModel : BaseViewModel
    {

        private ProductRepository productRepository = new ProductRepository();
        private MillimarBDRepository millimarBDRepository = new MillimarBDRepository();

        private List<Product> products;

        public List<Product> Products
        {
            get { return products; }
            set { products = value; OnPropertyChanged(); }
        }

        private List<TechnologyParamDto> technologyParamDtos;
        public List<TechnologyParamDto> TechnologyParamDtos
        {
            get { return technologyParamDtos; }
            set { technologyParamDtos = value; OnPropertyChanged(); }
        }

        private List<TechnologyDto> technologyDtos;
        public List<TechnologyDto> TechnologyDtos
        {
            get { return technologyDtos; }
            set { technologyDtos = value; OnPropertyChanged(); }
        }

        private bool technologyEnabled = false;

        public bool TechnologyEnabled
        {
            get { return technologyEnabled; }
            set { technologyEnabled = value; OnPropertyChanged(); }
        }
        public Action RequestClose { get; set; }
        public ICommand CloseCommand => new RelayCommand(() =>
        {
            RequestClose?.Invoke();
        });
        public ICommand ConfirmCommand => new RelayCommand(async () =>
        {
            GlobalSession.Instance.SelectedProduct = SelectedProduct;
            GlobalSession.Instance.CurrentSelectedTechnologyDtos = TechnologyDtos;
            GlobalSession.Instance.CurrentSelectedTechnologyDto = SelectedTechnologyDto;
            GlobalSession.Instance.CurrentTechnologyParamDtos = SelectedTechnologyDto.Params;
            var result = await millimarBDRepository.GetMillimarBDs();
            if (result != null && result.Count > 0)
            {
                GlobalSession.Instance.BdDatas = result.Select(x => x.Value).ToList();
            }
            Messenger.Send("ModelSelection", true);
            RequestClose?.Invoke();
        });
        public ICommand LoadedCommand { get; set; }

        private TechnologyDto selectedTechnologyDto;

        public TechnologyDto SelectedTechnologyDto
        {
            get { return selectedTechnologyDto; }
            set
            {
                selectedTechnologyDto = value;
                OnPropertyChanged();

            }
        }

        private Product selectedProduct;

        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                selectedProduct = value;
                OnPropertyChanged();
                ProductSelectionChanged();
            }
        }

        public ModelSelectionViewModel()
        {
            LoadedCommand = new RelayCommand(Loaded);
        }

        private void ProductSelectionChanged()
        {

            Task.Run(async () =>
            {
                var result = await productRepository.GetTechnologiesByProductIdAsync(SelectedProduct?.ProductId);
                TechnologyEnabled = result?.Technologies?.Count > 0;
                TechnologyDtos = result?.Technologies;
            });
        }

        private void Loaded()
        {
            Task.Run(async () =>
            {
                var products = await productRepository.GetPageAsync(1, 1000);
                Products = products.Items;
            });
        }
    }
}
