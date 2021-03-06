﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using TCC.Annotations;
using TCC.ViewModels;

namespace TCC.Controls
{
    /// <summary>
    /// Logica di interazione per LancerBuffCooldownControl.xaml
    /// </summary>
    public partial class BuffCooldownControl : UserControl, INotifyPropertyChanged
    {
        public BuffCooldownControl()
        {
            InitializeComponent();
        }

        DurationCooldownIndicator _context;
        private DoubleAnimation _anim;
        public string DurationLabel => _context == null? "": Utils.TimeFormatter(_context.Buff.Seconds);
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //externalArc.BeginAnimation(Arc.EndAngleProperty, new DoubleAnimation(359.9, 0, TimeSpan.FromMilliseconds(50000)));

            if (DesignerProperties.GetIsInDesignMode(this) || DataContext == null) return;
            _context = (DurationCooldownIndicator)DataContext;
            _context.Buff.PropertyChanged += Buff_PropertyChanged;
            _anim = new DoubleAnimation(359.9, 0, TimeSpan.FromMilliseconds(_context.Buff.Cooldown));
        }


        private void Buff_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.InvokeIfRequired(() =>
            {

                if (e.PropertyName == "Start")
                {
                    _anim.Duration = TimeSpan.FromMilliseconds(_context.Buff.Cooldown);
                    externalArc.BeginAnimation(Arc.EndAngleProperty, _anim);
                    return;
                }
                if (e.PropertyName == "Refresh")
                {
                    _anim.Duration = TimeSpan.FromMilliseconds(_context.Buff.Cooldown);
                    externalArc.BeginAnimation(Arc.EndAngleProperty, _anim);
                    return;
                }
                if (e.PropertyName == nameof(_context.Buff.Seconds))
                {
                    OnPropertyChanged(nameof(DurationLabel));
                }

            }, System.Windows.Threading.DispatcherPriority.DataBind);

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
