using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ActiveDirectoryQuerier.PowerShell;
using ActiveDirectoryQuerier.Queries;
using ActiveDirectoryQuerier.ViewModels;
using Microsoft.Win32;

namespace ActiveDirectoryQuerier;

public class QueryExecutor
(PSExecutor psExecutor, ConsoleViewModel consoleOutputInQueryBuilder, MainWindowViewModel mainWindowViewModel)
{
    /// <remarks>
    /// The use of the <c>command</c> parameter indicates that the method can be called from the Query Stack Panel.
    /// On the other hand, the absence of the <c>command</c> parameter and use of the
    /// <c>SelectedCommandInQueryBuilder</c> property indicates that the method can be called from the Query Builder.
    /// </remarks>
    public async Task ExecuteQueryAsync(ConsoleViewModel consoleOutput, Command? command = null)
    {
        if (mainWindowViewModel.SelectedCommandInQueryBuilder is null && command is null)
        {
            Trace.WriteLine("No command selected.");
            MessageBox.Show("To execute a command, you must first select a command.",
                            "Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        try
        {
            PSOutput result;
            if (command is not null)
            {
                result = await psExecutor.ExecuteAsync(command);
            }
            else
            {
                mainWindowViewModel.UpdateCommandWithSelectedParameters();
                // Null forgiveness operator is used because if command is not null, this line will never be reached.
                // If it is null, that must mean that SelectedCommandInQueryBuilder is not null, else the return
                // statement above would have been executed.
                result = await psExecutor.ExecuteAsync(mainWindowViewModel.SelectedCommandInQueryBuilder!);
            }

            if (result.HadErrors)
            {
                consoleOutput.Append(result.StdErr);
                return;
            }

            consoleOutput.Append(result.StdOut);
        }
        catch (Exception exception)
        {
            consoleOutput.Append($"Error executing command: {exception.Message}");
        }
    }

    public async Task OutputExecutionResultsToTextFileAsync(object? queryButton)
    {
        if (queryButton is not null)
        {
            var buttonQuery = (Query)((Button)queryButton).Tag;
            await ExecuteQueryAsync(consoleOutputInQueryBuilder, buttonQuery.Command);
        }
        else
        {
            await ExecuteQueryAsync(consoleOutputInQueryBuilder);
        }

        // Filepath
        // Write the text to a file & prompt user for the location
        SaveFileDialog saveFileDialog = new() {                       // Set properties of the OpenFileDialog
                                               FileName = "Document", // Default file name
                                               Filter = "All files(*.*) | *.*"
        };

        // Display
        bool? result = saveFileDialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, consoleOutputInQueryBuilder.ConsoleOutput);
        }
    }

    public async Task OutputExecutionResultsToCsvFileAsync(object? queryButton)
    {
        if (queryButton is not null)
        {
            var buttonQuery = (Query)((Button)queryButton).Tag;
            await ExecuteQueryAsync(consoleOutputInQueryBuilder, buttonQuery.Command);
        }
        else
        {
            await ExecuteQueryAsync(consoleOutputInQueryBuilder);
        }

        StringBuilder csv = new();
        string[] output = consoleOutputInQueryBuilder.ConsoleOutput.Split(' ', '\n');

        for (int i = 0; i < output.Length - 2; i++)
        {
            var first = output[i];
            var second = output[i + 1];
            // format the strings and add them to a string
            var newLine = $"{first},{second}";
            csv.AppendLine(newLine);
        }

        // Write the text to a file & prompt user for the location
        SaveFileDialog saveFileDialog = new() { FileName = "Document", Filter = "All files(*.*) | *.*" };

        // Display
        bool? result = saveFileDialog.ShowDialog();

        // Get file and write text
        if (result == true)
        {
            // Open document
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, csv.ToString());
        }
    }

    public void ExportConsoleOutputToFile(object _)
    {
        if (consoleOutputInQueryBuilder.ConsoleOutput.Length == 0)
        {
            MessageBox.Show("The console is empty.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SaveFileDialog saveFileDialog = new() { DefaultExt = ".txt", Filter = "Text documents (.txt)|*.txt" };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            string filename = saveFileDialog.FileName;
            consoleOutputInQueryBuilder.ExportToTextFile(filename);
        }
    }
}
