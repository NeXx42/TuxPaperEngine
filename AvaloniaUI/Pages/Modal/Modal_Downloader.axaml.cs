using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaUI.Interfaces;
using Logic;

namespace AvaloniaUI.Pages.Modal;

public partial class Modal_Downloader : UserControl, IModal
{
    private Dictionary<string, string> gitProjects = new Dictionary<string, string>()
    {
        { "Almamu (Upstream)", "https://github.com/Almamu/linux-wallpaperengine" },
        { "Nexx (Fork)", "https://github.com/NeXx42/linux-wallpaperengine-fork" },
    };

    private string? selectedDir;
    private bool inDownload;

    public Modal_Downloader()
    {
        InitializeComponent();

        btn_Act.RegisterClick(Install, "Installing");
        inp_Dir.RegisterClick(SelectInstallLocation);
        inp_GitProject.ItemsSource = gitProjects.Keys;
        inp_GitProject.SelectedIndex = 0;

        cont_Options.IsVisible = true;
        cont_Progress.IsVisible = false;

        inDownload = false;
    }

    public bool isBlocking => inDownload;

    private async Task Install()
    {
        if (string.IsNullOrEmpty(selectedDir))
            return;

        LinkedList<string> lines = new LinkedList<string>();

        cont_Options.IsVisible = false;
        cont_Progress.IsVisible = true;

        inDownload = true;

        await WallpaperSetter.InstallEngineLocally(gitProjects[(string)inp_GitProject.SelectedValue!], selectedDir!,
            new Progress<int>((i) => Dispatcher.UIThread.Invoke(() =>
            {
                progress_Main!.Value = i;

                string text = "";

                switch (i)
                {
                    case 0: text = "cloning"; break;
                    case 1: text = "make"; break;
                    case 2: text = "building"; break;
                }

                lbl_Step.Content = $"Downloading - {text}";
            })),
            new Progress<int>((i) => Dispatcher.UIThread.Invoke(() =>
            {
                progress_Task!.Value = i;
            })),
            (string msg) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    lines.AddLast(msg);

                    if (lines.Count > 5)
                        lines.RemoveFirst();

                    lbl_Console.Text = "";
                    foreach (string line in lines)
                        lbl_Console.Text += $"{line}\n";
                });

            });

        inDownload = false;

        cont_Progress.IsVisible = false;
        cont_Options.IsVisible = true;

        await MainWindow.CloseModal();
    }

    private async Task SelectInstallLocation()
    {
        var result = (await MainWindow.instance!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Engine Location"
        })).FirstOrDefault();

        if (result != null)
        {
            selectedDir = result.Path.AbsolutePath;
            inp_Dir.Label = selectedDir;
        }
    }
}