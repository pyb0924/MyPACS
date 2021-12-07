using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight;
namespace MyPACSViewer.ViewModel
{
    class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<OpenFileViewModel>();
            SimpleIoc.Default.Register<OpenFolderViewModel>();
            SimpleIoc.Default.Register<OpenQRViewModel>();
        }

        public OpenFileViewModel OpenFile => ServiceLocator.Current.GetInstance<OpenFileViewModel>();

        public OpenFolderViewModel OpenFolder => ServiceLocator.Current.GetInstance<OpenFolderViewModel>();

        public OpenQRViewModel OpenQR => ServiceLocator.Current.GetInstance<OpenQRViewModel>();

        public static void CleanUp()
        {

        }

    }
}
