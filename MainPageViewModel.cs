using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Xamarin.Forms;
using MobileApp.Views;
using System.Collections.ObjectModel;
using MobileApp.Services;
using MobileApp.Models;
using Prism;
using Xamarin.Essentials;
using Newtonsoft.Json;
using Acr.UserDialogs;

namespace MobileApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase, IActiveAware
    {
        #region Commands
        public DelegateCommand FilterBookingsCommand { get; private set; }
        public DelegateCommand ClearTextCommand { get; private set; }
        public DelegateCommand FocusIncomingCommand { get; set; }
        public DelegateCommand FocusConfirmedCommand { get; set; }
        public DelegateCommand FocusDoneCommand { get; private set; }
        public DelegateCommand FocusCanceledCommand { get; private set; }
        public DelegateCommand<Booking> ToBookingDetailCommand { get; private set; }
        public DelegateCommand OpenFilterCommand { get; private set; }
        public DelegateCommand ClickAzCommand { get; private set; }
        public DelegateCommand ClickZaCommand { get; private set; }
        public DelegateCommand ClickDateAscCommand { get; private set; }
        public DelegateCommand ClickDateDescCommand { get; private set; }
        public DelegateCommand AcceptFilterCommand { get; private set; }

        #endregion

        #region Props
        public Profile Session { get; set; }
        public DataTemplate BookingsList { get; set; }

        bool OrderByAz = false;
        bool OrderByZa = false;
        bool OrderByDateAsc = false;
        bool OrderByDateDesc = false;

        ObservableCollection<Booking> _bookings = new ObservableCollection<Booking>();
        public ObservableCollection<Booking> Bookings { get => _bookings; set => SetProperty(ref _bookings, value); }

        ObservableCollection<Booking> _confirmedBookings = new ObservableCollection<Booking>();
        public ObservableCollection<Booking> ConfirmedBookings { get => _confirmedBookings; set => SetProperty(ref _confirmedBookings, value); }

        ObservableCollection<Booking> _doneBookings = new ObservableCollection<Booking>();
        public ObservableCollection<Booking> DoneBookings { get => _doneBookings; set => SetProperty(ref _doneBookings, value); }

        ObservableCollection<Booking> _canceledBookings = new ObservableCollection<Booking>();
        public ObservableCollection<Booking> CanceledBookings { get => _canceledBookings; set => SetProperty(ref _canceledBookings, value); }

        int _position;
        public int Position { get => _position; set => SetProperty(ref _position, value); }

        public event EventHandler IsActiveChanged;
        bool _isActive;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value, GetBookings); }

        bool _cancelIconVisible;
        public bool CancelIconVisible { get => _cancelIconVisible; set => SetProperty(ref _cancelIconVisible, value); }

        string _queryString = "";
        public string QueryString { get => _queryString; set => SetProperty(ref _queryString, value, QueryChanged); }

        bool _isModal;
        public bool IsModal { get => _isModal; set => SetProperty(ref _isModal, value); }

        string _colorButtonAz = "#CFCACA";
        public string ColorButtonAz { get => _colorButtonAz; set => SetProperty(ref _colorButtonAz, value); }

        string _colorButtonZa = "#CFCACA";
        public string ColorButtonZa { get => _colorButtonZa; set => SetProperty(ref _colorButtonZa, value); }

        string _colorButtonDateAsc = "#CFCACA";
        public string ColorButtonDateAsc { get => _colorButtonDateAsc; set => SetProperty(ref _colorButtonDateAsc, value); }

        string _colorButtonDateDesc = "#CFCACA";
        public string ColorButtonDateDesc { get => _colorButtonDateDesc; set => SetProperty(ref _colorButtonDateDesc, value); }

        DateTime _selectedDate1;
        public DateTime SelectedDate1 { get => _selectedDate1; set => SetProperty(ref _selectedDate1, value); }

        DateTime _selectedDate2 = DateTime.UtcNow;
        public DateTime SelectedDate2 { get => _selectedDate2; set => SetProperty(ref _selectedDate2, value); }

        public string Main_entrantes { get => Resources.AppResources.Main_entrantes; }
        public string Main_Confirmadas { get => Resources.AppResources.Main_Confirmadas; }
        public string Main_Realizadas { get => Resources.AppResources.Main_Realizadas; }
        public string Main_Canceladas { get => Resources.AppResources.Main_Canceladas; }
        public string Main_Aunnotienes { get => Resources.AppResources.Main_Aunnotienes; }
        public string Main_Aquisemostraran { get => Resources.AppResources.Main_Aquisemostraran; }
        public string Main_Aunnotienes2 { get => Resources.AppResources.Main_Aunnotienes2; }
        public string Main_Aquisemostraran2 { get => Resources.AppResources.Main_Aquisemostraran2; }
        public string Main_Aunnotienes3 { get => Resources.AppResources.Main_Aunnotienes3; }
        public string Main_Aquisemostraran3 { get => Resources.AppResources.Main_Aquisemostraran3; }
        public string Main_Aunnotienes4 { get => Resources.AppResources.Main_Aunnotienes4; }
        public string Main_Aquisemostraran4 { get => Resources.AppResources.Main_Aquisemostraran4; }
        public string Main_Ordenar { get => Resources.AppResources.Main_Ordenar; }
        public string Main_Yate { get => Resources.AppResources.Main_Yate; }
        public string Core_Fecha { get => Resources.AppResources.Core_Fecha; }
        public string Main_A_Z { get => Resources.AppResources.Main_A_Z; }
        public string Main_Z_A { get => Resources.AppResources.Main_Z_A; }
        public string Main_Nuevo { get => Resources.AppResources.Main_Nuevo; }
        public string Main_Viejo { get => Resources.AppResources.Main_Viejo; }
        public string Main_Fechas { get => Resources.AppResources.Main_Fechas; }
        public string Core_Aceptar { get => Resources.AppResources.Core_Aceptar; }
        public string Core_buscar { get => Resources.AppResources.Core_buscar; }

        #endregion

        public MainPageViewModel(INavigationService navigationService, IAPIManager apiManager) : base(navigationService, apiManager)
        {
            FilterBookingsCommand = new DelegateCommand(FilterBookings);
            ClearTextCommand = new DelegateCommand(ClearText);
            FocusIncomingCommand = new DelegateCommand(FocusIncoming);
            FocusConfirmedCommand = new DelegateCommand(FocusConfirmed);
            FocusDoneCommand = new DelegateCommand(FocusDone);
            FocusCanceledCommand = new DelegateCommand(FocusCanceled);
            ToBookingDetailCommand = new DelegateCommand<Booking>(ToBookingDetail);
            ReloadCommand = new DelegateCommand(() => { IsReloading = true; GetBookings(); IsReloading = false; });
            OpenFilterCommand = new DelegateCommand(OpenFIlter);
            ClickAzCommand = new DelegateCommand(ClickAz);
            ClickZaCommand = new DelegateCommand(ClickZa);
            ClickDateAscCommand = new DelegateCommand(ClickDateAsc);
            ClickDateDescCommand = new DelegateCommand(ClickDateDesc);
            AcceptFilterCommand = new DelegateCommand(AcceptFilter);

            SelectedDate1 = SelectedDate2.AddMonths(-1);

            BookingsList = new DataTemplate(() =>
            {
                return new ProviderTemplate
                {
                    ParentContext = this
                };
            });
        }

        public async void GetBookings()
        {
            if (IsActive)
            {
                List<Booking> list = await APIManager.GetBookings(null);
                if (list != null)
                    Bookings = new ObservableCollection<Booking>(list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate));

                List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                if (ConfirmedList != null)
                    ConfirmedBookings = new ObservableCollection<Booking>(ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate));

                List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                if (DoneList != null)
                    DoneBookings = new ObservableCollection<Booking>(DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate));

                List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                if (CanceledList != null)
                    CanceledBookings = new ObservableCollection<Booking>(CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate));
            }
        }

        public void FocusIncoming()
        {
            Position = 0;
        }

        public void FocusConfirmed()
        {
            Position = 1;
        }

        public void FocusDone()
        {
            Position = 2;
        }

        public void FocusCanceled()
        {
            Position = 3;
        }

        public void ClearText()
        {
            QueryString = "";
            FilterBookings();
        }

        public void QueryChanged()
        {
            CancelIconVisible = !String.IsNullOrEmpty(QueryString);

            if (String.IsNullOrEmpty(QueryString) || QueryString.Length < 3)
                return;
        }

        public async void FilterBookings()
        {
            using (UserDialogs.Instance.Loading("Cargando..."))
            {
                if (string.IsNullOrEmpty(QueryString))
                {
                    if (OrderByAz || OrderByZa || OrderByDateAsc || OrderByDateDesc)
                    {
                        if (OrderByAz)
                        {
                            if (IsActive)
                            {
                                List<Booking> list = await APIManager.GetBookings(null);
                                if (list != null)
                                {
                                    var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.BoatName);
                                    Bookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                                if (ConfirmedList != null)
                                {
                                    var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.BoatName);
                                    ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                                if (DoneList != null)
                                {
                                    var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.BoatName);
                                    DoneBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                                if (CanceledList != null)
                                {
                                    var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.BoatName);
                                    CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                                }
                            }
                        }

                        if (OrderByZa)
                        {
                            if (IsActive)
                            {
                                List<Booking> list = await APIManager.GetBookings(null);
                                if (list != null)
                                {
                                    var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.BoatName);
                                    Bookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                                if (ConfirmedList != null)
                                {
                                    var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.BoatName);
                                    ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                                if (DoneList != null)
                                {
                                    var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.BoatName);
                                    DoneBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                                if (CanceledList != null)
                                {
                                    var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.BoatName);
                                    CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                                }
                            }
                        }

                        if (OrderByDateAsc)
                        {
                            if (IsActive)
                            {
                                List<Booking> list = await APIManager.GetBookings(null);
                                if (list != null)
                                {
                                    var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.DisplayDate);
                                    Bookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                                if (ConfirmedList != null)
                                {
                                    var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.DisplayDate);
                                    ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                                if (DoneList != null)
                                {
                                    var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.DisplayDate);
                                    DoneBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                                if (CanceledList != null)
                                {
                                    var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderBy(n => n.DisplayDate);
                                    CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                                }
                            }
                        }

                        if (OrderByDateDesc)
                        {
                            if (IsActive)
                            {
                                List<Booking> list = await APIManager.GetBookings(null);
                                if (list != null)
                                {
                                    var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate);
                                    Bookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                                if (ConfirmedList != null)
                                {
                                    var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate);
                                    ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                                if (DoneList != null)
                                {
                                    var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate);
                                    DoneBookings = new ObservableCollection<Booking>(ListOrder);
                                }

                                List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                                if (CanceledList != null)
                                {
                                    var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1)).OrderByDescending(n => n.DisplayDate);
                                    CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.GetBookings();
                    }
                    return;
                }
                if (OrderByAz || OrderByZa || OrderByDateAsc || OrderByDateDesc)
                {
                    if (OrderByAz)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }
                        }
                    }

                    if (OrderByZa)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                        }

                       
                    }
                    if (OrderByDateAsc)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }
                        }
                    }

                    if (OrderByDateDesc)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }
                        }
                    }
                    
                }
                else
                {
                    if (IsActive)
                    {
                        List<Booking> list = await APIManager.GetBookings(null);
                        if (list != null)
                        {
                            var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            Bookings = new ObservableCollection<Booking>(ListOrder);
                        }

                        List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                        if (ConfirmedList != null)
                        {
                            var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                        }

                        List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                        if (DoneList != null)
                        {
                            var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            DoneBookings = new ObservableCollection<Booking>(ListOrder);
                        }

                        List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                        if (CanceledList != null)
                        {
                            var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                        }
                    }
                }
            }
        }

        public async void ToBookingDetail(Booking book)
        {
            using (UserDialogs.Instance.Loading("Cargando..."))
            {
                if (string.IsNullOrEmpty(book.ClientEmail))
                    book.ClientEmail = "Sin correo electrónico";

                if (string.IsNullOrEmpty(book.ContactCelPhone))
                    book.ContactCelPhone = "Sin teléfono celular";

                //Pending 
                if (Position == 0)
                {
                    await NavigationService.NavigateAsync("MainPageDetail", new NavigationParameters { { "booking", book }, { "buttons", true }, { "position", Position } });
                    return;
                }

                //Accepted 
                if (Position == 1)
                {
                    await NavigationService.NavigateAsync("MainPageDetail", new NavigationParameters { { "booking", book }, { "accepted", true }, { "comment", true }, { "position", Position } });
                    return;
                }

                //Done
                if (Position == 2)
                {
                    await NavigationService.NavigateAsync("MainPageDetail", new NavigationParameters { { "booking", book }, { "accepted", true }, { "position", Position } });
                    return;
                }

                //Canceled 
                if (Position == 3)
                {
                    await NavigationService.NavigateAsync("MainPageDetail", new NavigationParameters { { "booking", book }, { "position", Position } });
                    return;
                }
                await NavigationService.NavigateAsync("MainPageDetail", new NavigationParameters { { "booking", book }, { "position", Position } });
            }
        }

        public void OpenFIlter()
        {
            IsModal = !IsModal;
        }

        public void ClickAz()
        {
            if (ColorButtonAz == "#CFCACA")
            {
                ColorButtonAz = "#0072A0";
                OrderByAz = true;

                if (ColorButtonZa == "#0072A0" || ColorButtonDateAsc == "#0072A0" || ColorButtonDateDesc == "#0072A0")
                {
                    ColorButtonZa = "#CFCACA";
                    ColorButtonDateAsc = "#CFCACA";
                    ColorButtonDateDesc = "#CFCACA";
                    OrderByZa = false;
                    OrderByDateAsc = false;
                    OrderByDateDesc = false;
                }
            }
            else
            {
                ColorButtonAz = "#CFCACA";
                OrderByAz = false;
            }
        }

        public void ClickZa()
        {
            if (ColorButtonZa == "#CFCACA")
            {
                ColorButtonZa = "#0072A0";
                OrderByZa = true;

                if (ColorButtonAz == "#0072A0" || ColorButtonDateAsc == "#0072A0" || ColorButtonDateDesc == "#0072A0")
                {
                    ColorButtonAz = "#CFCACA";
                    ColorButtonDateAsc = "#CFCACA";
                    ColorButtonDateDesc = "#CFCACA";
                    OrderByAz = false;
                    OrderByDateAsc = false;
                    OrderByDateDesc = false;
                }
            }
            else
            {
                ColorButtonZa = "#CFCACA";
                OrderByZa = false;
            }
        }

        public void ClickDateAsc()
        {
            if (ColorButtonDateAsc == "#CFCACA")
            {
                ColorButtonDateAsc = "#0072A0";
                OrderByDateAsc = true;

                if (ColorButtonAz == "#0072A0" || ColorButtonZa == "#0072A0" || ColorButtonDateDesc == "#0072A0")
                {
                    ColorButtonZa = "#CFCACA";
                    ColorButtonAz = "#CFCACA";
                    ColorButtonDateDesc = "#CFCACA";
                    OrderByZa = false;
                    OrderByAz = false;
                    OrderByDateDesc = false;
                }
            }
            else
            {
                ColorButtonDateAsc = "#CFCACA";
                OrderByDateAsc = false;
            }
        }

        public void ClickDateDesc()
        {
            if (ColorButtonDateDesc == "#CFCACA")
            {
                ColorButtonDateDesc = "#0072A0";
                OrderByDateDesc = true;

                if (ColorButtonAz == "#0072A0" || ColorButtonZa == "#0072A0" || ColorButtonDateAsc == "#0072A0")
                {
                    ColorButtonZa = "#CFCACA";
                    ColorButtonAz = "#CFCACA";
                    ColorButtonDateAsc = "#CFCACA";
                    OrderByZa = false;
                    OrderByAz = false;
                    OrderByDateAsc = false;
                }
            }
            else
            {
                OrderByDateDesc = false;
                ColorButtonDateDesc = "#CFCACA";
            }
        }

        public async void AcceptFilter()
        {
            if (SelectedDate1 > SelectedDate2)
            {
                await UserDialogs.Instance.AlertAsync("La fecha inicial no puede ser mayor a la fecha final", "¡Atención!", "Ok");
                return;
            }
            using (UserDialogs.Instance.Loading("Cargando..."))
            {
                if (OrderByAz || OrderByZa || OrderByDateAsc || OrderByDateDesc)
                {
                    if (OrderByAz)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.BoatName);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }
                        }
                    }

                    if (OrderByZa)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.BoatName);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }
                        }
                    }

                    if (OrderByDateAsc)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderBy(n => n.DisplayDate);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }
                        }
                    }

                    if (OrderByDateDesc)
                    {
                        if (IsActive)
                        {
                            List<Booking> list = await APIManager.GetBookings(null);
                            if (list != null)
                            {
                                var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                Bookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                            if (ConfirmedList != null)
                            {
                                var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                            if (DoneList != null)
                            {
                                var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                DoneBookings = new ObservableCollection<Booking>(ListOrder);
                            }

                            List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                            if (CanceledList != null)
                            {
                                var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                                CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                            }
                        }
                    }
                }
                else
                {
                    if (IsActive)
                    {
                        List<Booking> list = await APIManager.GetBookings(null);
                        if (list != null)
                        {
                            var ListOrder = list.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            Bookings = new ObservableCollection<Booking>(ListOrder);
                        }

                        List<Booking> ConfirmedList = await APIManager.GetBookingsByStatus(null, 3);
                        if (ConfirmedList != null)
                        {
                            var ListOrder = ConfirmedList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            ConfirmedBookings = new ObservableCollection<Booking>(ListOrder);
                        }

                        List<Booking> DoneList = await APIManager.GetBookingsByStatus(null, 5);
                        if (DoneList != null)
                        {
                            var ListOrder = DoneList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            DoneBookings = new ObservableCollection<Booking>(ListOrder);
                        }

                        List<Booking> CanceledList = await APIManager.GetBookingsByStatus(null, 6);
                        if (CanceledList != null)
                        {
                            var ListOrder = CanceledList.Where(n => n.DisplayDate >= SelectedDate1 && n.DisplayDate <= SelectedDate2.AddDays(1) && (n.BoatName.Contains(QueryString.ToUpper()) || n.BookingCode.Contains(QueryString.ToUpper()))).OrderByDescending(n => n.DisplayDate);
                            CanceledBookings = new ObservableCollection<Booking>(ListOrder);
                        }
                    }
                }
            }
            this.OpenFIlter();
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            Position = parameters["position"] is null ? Position : (int)parameters["position"];
        }
    }
}
