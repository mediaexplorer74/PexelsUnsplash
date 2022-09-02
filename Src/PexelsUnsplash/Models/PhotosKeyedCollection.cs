using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Stream.Wall.Models {
    public class PhotosKeyedCollection : KeyedCollection<string, Unsplasharp.Models.Photo>,
        INotifyCollectionChanged, INotifyPropertyChanged {
        
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public IList<Unsplasharp.Models.Photo> Photos { get => Items; }

        protected override string GetKeyForItem(Unsplasharp.Models.Photo item) {
            return item.Id;
        }

        protected override void ClearItems() {
            base.ClearItems();
            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        protected override void InsertItem(int index, Unsplasharp.Models.Photo item) {
            base.InsertItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected override void RemoveItem(int index) {
            var item = Items[index];
            base.RemoveItem(index);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        protected override void SetItem(int index, Unsplasharp.Models.Photo item) {
            base.SetItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, item, index);
        }

        #region events
        private void NotifyPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, Unsplasharp.Models.Photo item, int index) {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction action) {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }
        #endregion events
    }
}
