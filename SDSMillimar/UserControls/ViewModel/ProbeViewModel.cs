using SDSMillimar.Common;
using SDSMillimar.Models;

namespace SDSMillimar.UserControls.ViewModel
{
    public class ProbeViewModel:BaseViewModel
    {
        private OptionsModel selectedChannel;

        public OptionsModel SelectedChannel
        {
            get { return selectedChannel; }
            set
            {
                selectedChannel = value;
            }
        }
        public ProbeViewModel()
        {
                
        }
    }
}
