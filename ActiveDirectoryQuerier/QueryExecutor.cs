using System.Diagnostics;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Reflection;
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
            OutputFormat outputFormat = CalculateOutputFormat();

            if (command is not null)
            {
                result = await psExecutor.ExecuteAsync(command, outputFormat);
            }
            else
            {
                mainWindowViewModel.UpdateCommandWithSelectedParameters();
                // Null forgiveness operator is used because if command is not null, this line will never be reached.
                // If it is null, that must mean that SelectedCommandInQueryBuilder is not null, else the return
                // statement above would have been executed.
                result =
                    await psExecutor.ExecuteAsync(mainWindowViewModel.SelectedCommandInQueryBuilder!, outputFormat);
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

    public async Task OutputExecutionResultsToTextFileAsync(object? queryButton)
    {
        await OutputExecutionResultsToFileAsync(queryButton, ".txt", "Text documents (.txt)|*.txt");
    }

    public async Task OutputExecutionResultsToCsvFileAsync(object? queryButton)
    {
        await OutputExecutionResultsToFileAsync(queryButton, ".csv", "CSV files (*.csv)|*.csv");
    }
    
    private async Task OutputExecutionResultsToFileAsync(object? queryButton, string fileExtension, string filter)
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

        SaveFileDialog saveFileDialog = new() { DefaultExt = fileExtension, Filter = filter };

        bool? result = saveFileDialog.ShowDialog();

        if (result == true)
        {
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, consoleOutputInQueryBuilder.ConsoleOutput);
        }
    }
    
    private OutputFormat CalculateOutputFormat()
    {
        StackTrace stackTrace = new();
        StackFrame[] stackFrames = stackTrace.GetFrames();

        try
        {
            if (stackFrames.Length > 2)
            {
                // Null forgiveness operator is used because the method this statement would not be reached if the
                // length of the stackFrames array is less than 3.
                MethodBase callingMethod = stackFrames[2].GetMethod()!;
                if (callingMethod.Name == "OutputExecutionResultsToCsvFileAsync")
                {
                    return OutputFormat.Csv;
                }
            }

            return OutputFormat.Text;
        }
        catch (Exception)
        {
            return OutputFormat.Text;
        }
    }
}
