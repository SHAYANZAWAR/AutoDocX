
using System.CommandLine.Completions;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using AutoDocx;


namespace AutoDocXCLI
{
    class _AutoDocXCLI
    {
        static async Task Main(string[] args)
        {

            // defining the main command line interface for "AutoDocX"

            var rootCommand = new RootCommand("autodocx");
            rootCommand.Description = "A command line tool that can automate the manual work of adding the outputs of your c++ program into a wordFile";
            // declaring the option to add path of word file to autodocx
            var wordFilePathArg = new Option<string>(
                name: "--wordFile",
                description: "option to add path of word file to autodocx",
                getDefaultValue: () => "out.docx"
            );
            // defining the subcommands (add, remove, update)
            var addCommand = new Command("add", "subcommand to add filePath to autodocx.");
            var updateCommand = new Command("update", "subcommand to update an output in the wordFile.");
            var removeCommand = new Command("remove", "subcommand to remove an output in the wordFile.");


            // defining arguments to subcommand (add)
            var filePathArg = new Argument<string>(
                name: "FilePath",
                description: "Argument to add filePath to autodocx."
            );
            filePathArg.Arity = ArgumentArity.ExactlyOne;

            var outputHeading = new Argument<string>(
                name: "FileOutputHeading",
                description: "Argument to specify heading of output in word file.",
                getDefaultValue: () => "default"
            );
            outputHeading.Arity = ArgumentArity.ZeroOrOne;

            var multipleFileStructureFlag = new Option<string>(
                name: "--mfile",
                description: "flag to specify that input file depends on multiple.",
                getDefaultValue: () => ""
            ); 
            multipleFileStructureFlag.Arity = ArgumentArity.Zero;

            var avoidFilesOption = new Option<string>(
                name: "--not",
                description: "Option to specify the files to avoid adding in case of multiple file dependencies.",
                getDefaultValue: () => ""
            )
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
            
            // defining arguments to subcommand (update)
            var oldFileArg = new Argument<string>(
                name: "OldFileName",
                description: "Argument to add old file path that would be searched in the provided wordFile to be update"
            )
            {
                Arity = ArgumentArity.ExactlyOne
            };


            var newFileArg = new Argument<string>(
                name: "NewFileName",
                description: "Argument to add new file path to be updated with the old file"
            )
            {
                Arity = ArgumentArity.ExactlyOne
            };


            // Binding options to the global command (root: autodocx)
            rootCommand.AddGlobalOption(wordFilePathArg);
            rootCommand.AddGlobalOption(multipleFileStructureFlag);
            rootCommand.AddGlobalOption(avoidFilesOption);

            // Binding arguments to subcommand
            addCommand.AddArgument(filePathArg);
            addCommand.AddArgument(outputHeading);
            updateCommand.AddArgument(oldFileArg);
            updateCommand.AddArgument(newFileArg);
            removeCommand.AddArgument(filePathArg);

            bool isMultipleFile = false;
            // adding validators for the subocmmands
            addCommand.AddValidator((result) =>
                {
                    if (!result.Children.Any(child => child.Symbol == filePathArg))
                    {
                        _AutoDocX.logError("Error: File path argument is required.");
                        return;
                    }
                    if (result.Children.Any(child => child.Symbol == multipleFileStructureFlag))
                    {
                        isMultipleFile = true;
                    }
                });

            updateCommand.AddValidator((result) =>
            {

                if (!result.Children.Any(child => child.Symbol == oldFileArg))
                {
                    _AutoDocX.logError("Error: Old File path argument is required");
                }

                if (!result.Children.Any(child => child.Symbol == newFileArg))
                {
                    _AutoDocX.logError("Error: New File path argument is required");

                }
                if (result.Children.Any(child => child.Symbol == multipleFileStructureFlag))
                {
                    isMultipleFile = true;
                }
            });


            // Setting handlers to subcommands
            addCommand.SetHandler((wordFilePathValue, filePathValue, outputHeadingValue, avoidFiles) =>
            {

                if (string.IsNullOrEmpty(filePathValue))
                {
                    _AutoDocX.logError("Error: no path to file provided");
                    return;
                }

                _AutoDocX.addToDoc(wordFilePathValue, filePathValue, outputHeadingValue, isMultipleFile, (String.IsNullOrEmpty(avoidFiles)) ? null : avoidFiles);

            }, wordFilePathArg, filePathArg, outputHeading, avoidFilesOption);



            updateCommand.SetHandler((wordFilePathValue, oldFilePath, newFilePath, avoidFiles) =>
            {

                _AutoDocX.updateInDocx(isMultipleFile, wordFilePathValue, oldFilePath, newFilePath, avoidFiles);


            }, wordFilePathArg, oldFileArg, newFileArg, avoidFilesOption);



            removeCommand.SetHandler((wordFilePathValue, filePath) =>
            {

                _AutoDocX.RemoveInDocX(wordFilePathValue, filePath);

            }, wordFilePathArg, filePathArg);



            // Binding subcommands to root command
            rootCommand.Add(addCommand);
            rootCommand.Add(updateCommand);
            rootCommand.Add(removeCommand);


            var commandLine = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build();


            await commandLine.InvokeAsync(args);

        }
    }
}