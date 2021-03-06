﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using TCC.ViewModels;


namespace TCC.Data
{
    public class Npc : TSPropertyChanged, IDisposable
    {
        public ulong EntityId { get; set; }
        protected string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }

        public bool IsBoss { get; set; }
        protected SynchronizedObservableCollection<AbnormalityDuration> _buffs;
        public SynchronizedObservableCollection<AbnormalityDuration> Buffs
        {
            get { return _buffs; }
            set
            {
                if (_buffs == value) return;
                _buffs = value;
                NotifyPropertyChanged("Buffs");
            }
        }

        protected bool enraged;
        public bool Enraged
        {
            get => enraged;
            set
            {
                if (enraged == value) return;
                enraged = value;
                NotifyPropertyChanged(nameof(Enraged));
            }
        }

        protected float _maxHP;
        public float MaxHP
        {
            get => _maxHP;
            set
            {
                if (_maxHP != value)
                {
                    _maxHP = value;
                    NotifyPropertyChanged("MaxHP");
                }
            }
        }
        protected float _currentHP;
        public float CurrentHP
        {
            get => _currentHP;
            set
            {
                if (_currentHP != value)
                {
                    _currentHP = value;
                    NotifyPropertyChanged(nameof(CurrentHP));
                    NotifyPropertyChanged(nameof(CurrentPercentage));
                    NotifyPropertyChanged(nameof(CurrentFactor));
                }
            }
        }
        private uint maxShield;
        public uint MaxShield
        {
            get => maxShield;
            set
            {
                if (maxShield != value)
                {
                    maxShield = value;
                    NotifyPropertyChanged(nameof(MaxShield));
                    NotifyPropertyChanged(nameof(ShieldFactor));
                }
            }
        }
        private float currentShield;
        public float CurrentShield
        {
            get => currentShield;
            set
            {
                if (currentShield == value) return;
                currentShield = value;
                NotifyPropertyChanged(nameof(CurrentShield));
                NotifyPropertyChanged(nameof(ShieldFactor));
            }
        }

        public double ShieldFactor => MaxShield > 0 ? CurrentShield / MaxShield : 0;

        public float CurrentFactor => _maxHP == 0 ? 0 : (_currentHP / _maxHP);
        public float CurrentPercentage => CurrentFactor * 100;
        protected Visibility visible;
        public Visibility Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    NotifyPropertyChanged("Visible");
                }
            }
        }

        protected ulong target;
        public ulong Target
        {
            get { return target; }
            set
            {
                if (target != value)
                {
                    target = value;
                    NotifyPropertyChanged("Target");
                }
            }
        }

        protected AggroCircle currentAggroType = AggroCircle.None;
        public AggroCircle CurrentAggroType
        {
            get { return currentAggroType; }
            set
            {
                if (currentAggroType != value)
                {
                    currentAggroType = value;
                    NotifyPropertyChanged("CurrentAggroType");
                }
            }
        }

        public uint ZoneId { get; protected set; }
        public uint TemplateId { get; protected set; }

        public EnragePattern EnragePattern { get; private set; }
        public void AddorRefresh(Abnormality ab, uint duration, int stacks)
        {
            var existing = Buffs.FirstOrDefault(x => x.Abnormality.Id == ab.Id);
            if (existing == null)
            {
                var newAb = new AbnormalityDuration(ab, duration, stacks, target, _dispatcher, true);
                if (ab.Infinity) Buffs.Insert(0, newAb);
                else Buffs.Add(newAb);
                if (ab.IsShield)
                {
                    MaxShield = ab.ShieldSize;
                    CurrentShield = ab.ShieldSize;
                }
                return;
            }
            existing.Duration = duration;
            existing.DurationLeft = duration;
            existing.Stacks = stacks;
            existing.Refresh();

        }
        public void EndBuff(Abnormality ab)
        {
            try
            {
                var buff = Buffs.FirstOrDefault(x => x.Abnormality.Id == ab.Id);
                if (buff == null) return;

                Buffs.Remove(buff);
                buff.Dispose();
                if (ab.IsShield)
                {
                    CurrentShield = 0;
                    MaxShield = 0;
                }

            }
            catch (Exception)
            {
            }
        }

        public bool IsTower => Utils.IsGuildTower(ZoneId, TemplateId);
        public bool IsPhase1Dragon => Utils.IsPhase1Dragon(ZoneId, TemplateId);

        public uint GuildId
        {
            get
            {
                BossGageWindowViewModel.Instance.GuildIds.TryGetValue(EntityId, out var val);
                return val;
            }
        }

        //public Npc(ulong eId, uint zId, uint tId, float curHP, float maxHP, Visibility visible)
        //{
        //    _dispatcher = BossGageWindowViewModel.Instance.GetDispatcher();
        //    EntityId = eId;
        //    Name = EntitiesManager.CurrentDatabase.GetName(tId, zId);
        //    ZoneId = zId;
        //    TemplateId = tId;
        //    MaxHP = maxHP;
        //    CurrentHP = curHP;
        //    _buffs = new SynchronizedObservableCollection<AbnormalityDuration>(_dispatcher);
        //    Visible = visible;

        //    IsShieldPhase = false;
        //    IsSelected = false;
        //    if (BossGageWindowViewModel.Instance.CurrentHHphase == HarrowholdPhase.Phase1)
        //    {
        //        ShieldDuration = new DispatcherTimer();
        //        ShieldDuration.Interval = TimeSpan.FromSeconds(13);
        //        ShieldDuration.Tick += ShieldFailed;
        //    }

        //}
        public Npc(ulong eId, uint zId, uint tId, bool boss, Visibility visible)
        {
            _dispatcher = BossGageWindowViewModel.Instance.GetDispatcher();
            EntityId = eId;
            Name = EntitiesManager.CurrentDatabase.GetName(tId, zId);
            MaxHP = EntitiesManager.CurrentDatabase.GetMaxHP(tId, zId);
            ZoneId = zId;
            IsBoss = boss;
            TemplateId = tId;
            CurrentHP = MaxHP;
            _buffs = new SynchronizedObservableCollection<AbnormalityDuration>(_dispatcher);
            Visible = visible;
            Shield = ShieldStatus.Off;
            IsSelected = true;
            EnragePattern = new EnragePattern(10, 36);
            if (IsPhase1Dragon)
            {
                ShieldDuration = new Timer();
                ShieldDuration.Interval = BossGageWindowViewModel.Ph1ShieldDuration*1000;
                ShieldDuration.Elapsed += ShieldFailed;

                EnragePattern.Duration = 50;
                EnragePattern.Percentage = 14;
            }
        }
        public Npc() { }
        public override string ToString()
        {
            return String.Format("{0} - {1}", EntityId, Name);
        }

        public void Dispose()
        {
            foreach (var buff in _buffs) buff.Dispose();
            ShieldDuration?.Dispose();
        }

        ///////////////////////////////////////////
        Timer ShieldDuration;

        private ShieldStatus _shield;
        public ShieldStatus Shield
        {
            get => _shield;
            set
            {
                if (_shield == value) return;
                _shield = value;
                NotifyPropertyChanged(nameof(Shield));
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value) return;
                isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected));
            }
        }
        private void ShieldFailed(object sender, EventArgs e)
        {
            ShieldDuration.Stop();
            Shield = ShieldStatus.Failed;
        }
        public void BreakShield()
        {
            ShieldDuration.Stop();
            Shield = ShieldStatus.Broken;
            Task.Delay(5000).ContinueWith(t =>
            {
                Shield = ShieldStatus.Off;
            });
        }
        public void StartShield()
        {
            ShieldDuration.Start();
            Shield = ShieldStatus.On;
        }

        public event Action DeleteEvent;
        public void Delete()
        {
            DeleteEvent?.Invoke();
        }
    }
}
