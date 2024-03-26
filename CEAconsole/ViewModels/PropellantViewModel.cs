using CEAconsole.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEAconsole.ViewModels
{
    public class PropellantViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<Reactant> _reactants;
        public ObservableCollection<Reactant> ReactantsCollection
        {
            get { return _reactants; }
            set
            {
                if (_reactants != value)
                {
                    _reactants = value;
                    OnPropertyChanged(nameof(Reactant));
                }
            }

        }

        public PropellantViewModel()
        {
            ReactantsCollection = new ObservableCollection<Reactant>();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddItem(Reactant reactant)
        {
            ReactantsCollection.Add(reactant);
        }

    }
}
