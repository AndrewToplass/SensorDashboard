using System.Linq;
using System.Runtime.Intrinsics.X86;
using Avalonia.Controls;
using SensorDashboard.ViewModels;

namespace SensorDashboard.Views;

public partial class FileTabView : UserControl
{
    public FileTabView()
    {
        InitializeComponent();
        DataContextChanged += delegate
        {
            if (DataContext is not FileTabViewModel vm)
            {
                return;
            }

            // Can't use a one-way binding for read-only property.
            foreach (var old in vm.DataGridColumns.OfType<DataGridBoundColumn>())
            {
                // Make sure existing dynamic columns are added back, need to be
                // recreated.
                var column = new DataGridTextColumn
                {
                    Header = old.Header,
                    Binding = old.Binding,
                    Width = old.Width,
                    IsReadOnly = old.IsReadOnly,
                    IsVisible = old.IsVisible,
                    CanUserSort = old.CanUserSort,
                    CanUserReorder = old.CanUserReorder,
                    CanUserResize = old.CanUserResize
                };
                DataGridDisplay.Columns.Add(column);
            }
            
            vm.DataGridColumns = DataGridDisplay.Columns;
        };
    }
}
