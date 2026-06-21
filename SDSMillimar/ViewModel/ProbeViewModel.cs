using SDSMillimar.Common;
using SDSMillimar.Models;
using SDSMillimar.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SDSMillimar.ViewModel
{
    public class ProbeViewModel : BaseViewModel
    {
        //public ObservableCollection<ProbeItem> Probes { get; }
        public ICommand ProbeClickCommand { get; set; }
        public Action RequestClose { get; set; }

        private bool confirmIsCan = false;

        public bool ConfirmIsCan
        {
            get { return confirmIsCan; }
            set { confirmIsCan = value; OnPropertyChanged(); }
        }

        private OptionsModel selectedChannel;

        public OptionsModel SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                selectedChannel = value; OnPropertyChanged();
            }
        }



        public ICommand CloseCommand => new RelayCommand(() =>
        {
            RequestClose?.Invoke();
        });
        public ICommand ConfirmCommand { get; set; }
        public ProbeViewModel()
        {
            ProbeClickCommand = new RelayCommand<ProbeItem>(ProbeClick);
            ConfirmCommand = new RelayCommand(Confirm);
            //Probes = new ObservableCollection<ProbeItem>()
            //{
            //    new ProbeItem() { Direction=1, Title="探针A", Id=1, GridColumn=0, GridRow=0, IsSelected=false},
            //    new ProbeItem() { Direction=-1, Title="探针B", Id=2, GridColumn=1, GridRow=0, IsSelected=false},
            //    new ProbeItem() { Direction=1, Title="探针C", Id=3, GridColumn=0, GridRow=1, IsSelected=false},
            //    new ProbeItem() { Direction=-1, Title="探针D", Id=4, GridColumn=1, GridRow=1, IsSelected=false},
            //    new ProbeItem() { Direction=1, Title="探针E", Id=5, GridColumn=0, GridRow=2, IsSelected=false},
            //    new ProbeItem() { Direction=-1, Title="探针F", Id=6, GridColumn=1, GridRow=2, IsSelected=false},
            //    new ProbeItem() { Direction=1, Title="探针G", Id=7, GridColumn=0, GridRow=3, IsSelected=false},
            //    new ProbeItem() { Direction=-1, Title="探针H", Id=8, GridColumn=1, GridRow=3, IsSelected=false},
            //    new ProbeItem() { Direction=1, Title="探针I", Id=9, GridColumn=0, GridRow=4, IsSelected=false},
            //    new ProbeItem() { Direction=-1, Title="探针J", Id=10, GridColumn=1, GridRow=4, IsSelected=false},
            //};
        }

        private void Closed()
        {
        }

        private void Confirm()
        {
            Messenger.Send("GetSelectedProbes", new ObservableCollection<ProbeItem>(GlobalSession.Instance.Probes.Where(x => x.IsSelected)));
            RequestClose?.Invoke();
        }

        private void ProbeClick(ProbeItem item)
        {
            GlobalSession.Instance.Probes.Where(x => x.Id == item.Id).First().IsSelected = !item.IsSelected;
            if (GlobalSession.Instance.Probes.Where(x => x.IsSelected).Count() == 2)
                ConfirmIsCan = true;
            else
            {
                ConfirmIsCan = false;

            }
        }
    }
}
