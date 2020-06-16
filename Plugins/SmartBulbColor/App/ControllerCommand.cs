using System;
using System.Windows.Input;

namespace SmartBulbColor.PluginApplication
{
    class ControllerCommand : ICommand
    {
        readonly Action<Object> DeligateExecute;
        readonly Predicate<Object> DeligateCanExecute;

        public ControllerCommand(Action<Object> execute) : this(execute, null)
        {

        }
        public ControllerCommand(Action<Object> execute, Predicate<Object> canExecute)
        {
            if(execute == null)
            {
                throw new Exception("Command can't execute, command is null");
            }
            DeligateExecute = execute;
            DeligateCanExecute = canExecute;
        }
        public bool CanExecute(Object parametr)
        {
            return DeligateCanExecute == null ? true : DeligateCanExecute.Invoke(parametr);
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        public void Execute(Object parametr)
        {
            DeligateExecute.Invoke(parametr);
        }
    }
}
