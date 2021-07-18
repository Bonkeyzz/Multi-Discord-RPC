/// ArgsParser
/// Author: Bonkey
/// Created at: 7/18/2021
/// Version: 1.0 

using System;
using System.Collections.Generic;
using System.Linq;

namespace Multi_DiscordRPC
{
    class Argument
    {
        public string Arg { get; }
        public string ShortenedArg { get; }
        public string Description { get; }
        public bool NeedsValue { get; }
        public string Value;
        public bool Exists { get; set; }
        public Argument(string arg, string shortArg, string description, bool valReq)
        {
            Arg = arg;
            ShortenedArg = shortArg;
            Description = description;
            NeedsValue = valReq;
            Exists = false;
        }

        public string GetValue()
        {
            return Value;
        }

        public void SetValue(string value)
        {
            Value = value;
        }
    }
    class ArgsParser
    {
        private string[] Arguments;
        private List<Argument> definedArgs;
        public ArgsParser()
        {
            definedArgs = new List<Argument>();
            AddArgument("help", "h", "Shows this help message");
        }

        public void AddArgument(string argument, string shortArg, string description, bool needsValue = false)
        {
            argument = argument.ToLower();
            definedArgs.Add(new Argument(argument, shortArg, description, needsValue));
        }

        private bool isProperArg(string arg)
        {
            return arg.StartsWith("--") || arg.StartsWith("-");
        }
        public bool Exists(string argName)
        {
            argName = argName.ToLower();
            if (HasValue(argName))
                argName = argName.Split('=')[0];
            return definedArgs.Find(x => x.Arg == argName || x.ShortenedArg == argName).Exists;
        }

        private bool HasValue(string argName)
        {
            return argName.Contains('=') && argName.Split('=')[1] != "";
        }
        public string GetValue(string argumentName)
        {
            argumentName = argumentName.ToLower();
            if (Exists(argumentName))
            {
                string noDashArg = argumentName.Replace("--", "").Replace("-", "");
                return definedArgs.Find(x => x.Arg == noDashArg || x.ShortenedArg == noDashArg).GetValue();
            }
            return null;
        }

        public void ParseArgs(string[] argsList)
        {
            Arguments = argsList;
            foreach (var argument in Arguments)
            {
                if (isProperArg(argument))
                {
                    string noDashArg = argument.Replace("--", "").Replace("-", "");
                    if (noDashArg == "help" || noDashArg == "h")
                    {
                        PrintHelp();
                        return;
                    }
                    string argOnly = noDashArg.Split('=')[0];
                    if (definedArgs.Exists(x => x.Arg == (noDashArg.Contains('=') ? argOnly : noDashArg) || x.ShortenedArg == (noDashArg.Contains('=') ? argOnly : noDashArg)))
                    {
                        Argument selectedArg = definedArgs.Find(x =>
                                x.Arg == (noDashArg.Contains('=') ? argOnly : noDashArg) ||
                                x.ShortenedArg == (noDashArg.Contains('=') ? argOnly : noDashArg));
                        selectedArg.Exists = true;

                        if (argument.Contains('='))
                        {
                            string value = argument.Split('=')[1];
                            selectedArg.SetValue(value);
                        }
                        else
                        {
                            if (selectedArg.NeedsValue)
                                throw new Exception($"Argument: '{selectedArg.Arg}' expected a value.");

                        }
                        definedArgs[definedArgs.IndexOf(definedArgs.Find(x => x.Arg == selectedArg.Arg))] = selectedArg;
                    }
                }
            }
        }

        public void PrintHelp(bool exit = true)
        {
            string programName = AppDomain.CurrentDomain.FriendlyName;

            Utils.PrettyPrint($"Usage: {programName} [options]\n", ConsoleColor.Gray);
            Utils.PrettyPrint("Arguments:", ConsoleColor.Gray);
            Utils.PrettyPrint("--help, -h\t\tShows this help message.", ConsoleColor.Gray);
            foreach (var arg in definedArgs)
            {
                if(arg.Arg == "help") continue;
                Utils.PrettyPrint($"--{arg.Arg}, -{arg.ShortenedArg}\t\t{arg.Description}", ConsoleColor.Gray);
            }
            if (exit) Environment.Exit(0);
        }
#if DEBUG
        public void DbgPrintArgs()
        {

            Utils.PrettyPrint("[DEBUG] === ARGUMENT LIST ===", ConsoleColor.Cyan);
            foreach (var arg in definedArgs)
            {
                Utils.PrettyPrint($"Arg: '{arg.Arg}'\nShortArg: '{arg.ShortenedArg}'\nDesc: '{arg.Description}'\nExists: {arg.Exists}\nValue Required: {arg.NeedsValue}\nValue: {arg.Value}", ConsoleColor.Cyan);
                Utils.PrettyPrint("====================================", ConsoleColor.Cyan);
            }
        }
#endif
    }
}
