using Microsoft.Win32;
using SDSMillimar.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SDSMillimar.View
{
    /// <summary>
    /// SPCAnalyseView.xaml 的交互逻辑
    /// </summary>
    public partial class SPCAnalyseView : Window
    {
        private SPCAnalyseViewModel sPCAnalyseViewModel;

        public SPCAnalyseView()
        {
            InitializeComponent();
            sPCAnalyseViewModel = new SPCAnalyseViewModel();
            this.DataContext = sPCAnalyseViewModel;
            // 指定委托，动态生成列
            sPCAnalyseViewModel.UpdateColumns = GenerateDataGridColumns;

            sPCAnalyseViewModel.ExportBmpRequested = () =>
            {
                SaveGridToBmpWithDialog(SpcRootGrid);
            };

            // 初次生成列
            GenerateDataGridColumns();
        }

        private void SaveGridToBmpWithDialog(FrameworkElement element)
        {
            if (element == null)
            {
                MessageBox.Show("未找到要截图的界面");
                return;
            }

            // ⚠️ 确保已经渲染完成
            element.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            int width = (int)Math.Ceiling(element.ActualWidth);
            int height = (int)Math.Ceiling(element.ActualHeight);

            if (width <= 0 || height <= 0)
            {
                MessageBox.Show("界面尚未完成布局");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "保存 SPC 控制图",
                Filter = "位图文件 (*.bmp)|*.bmp",
                DefaultExt = ".bmp",
                FileName = $"SPC_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() != true)
                return;

            var rtb = new RenderTargetBitmap(
                width,
                height,
                96, 96,
                PixelFormats.Pbgra32);

            rtb.Render(element);

            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = new FileStream(dialog.FileName, FileMode.Create))
            {
                encoder.Save(fs);
            }

            MessageBox.Show("保存成功");
        }




        private void GenerateDataGridColumns()
        {
            dataGridSubgroups.Columns.Clear();

            // 第一列显示子组名
            dataGridSubgroups.Columns.Add(new DataGridTextColumn
            {
                Header = "子组",
                Binding = new Binding("SubgroupName")
            });

            // 动态生成样本列
            for (int i = 0; i < sPCAnalyseViewModel.SubgroupNum; i++)
            {
                int index = i; // 必须用局部变量
                dataGridSubgroups.Columns.Add(new DataGridTextColumn
                {
                    Header = $"样本{i + 1}",
                    Binding = new Binding($"Samples[{index}]")
                });
            }
        }
    }
}
