using LT.Logger;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace LT_Nemesis
{
    public class NemesisMissionVM : ViewModel
    {

        //Mission _mission;
        //int _soundIndex;

        string _heroName;
        bool _isVisible = false;
        bool _isVisibleImage = false;
        //bool _isHidden = true;

        float _screenPositionX;
        float _screenPositionY;

        float _alphaFactor;
        int _fontSize;

        //float _iconSize;

        private float _width = 0f;
        private float _height = 0f;

        string _color = "#FFFFFFFF";

        string _voiceLineText;

        //public float IconSize = 45;

        //private Vec2 _screenPosition;

        //private bool _isBehind;

        private CharacterImageIdentifierVM _imageIdentifier;
        //private ImageIdentifierVM _banner;

        public NemesisMissionVM(Mission mission)
        {
            //_mission = mission;
            //var itemRoster = MobileParty.MainParty.ItemRoster;
            //var artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            //BeerAmount = itemRoster.GetItemNumber(artisanBeerObject);
            //_soundIndex = SoundEvent.GetEventIdFromString("artisanbeer/drink");

            OnMissionModeChanged(mission);

            _isVisible = false;
            _isVisibleImage = false;
            //_isHidden = true;
            _heroName = "";

            _alphaFactor = 0f;

            _fontSize = 12;

            _voiceLineText = "";


            //_iconSize = 25f;

            //_screenPositionX = "0";
            //_screenPositionY = "0";

            //this.RefreshValues(); // does not work

            //_screenPosition = new Vec2(1000f, 500f);

            _imageIdentifier = null; // new CharacterImageIdentifierVM(null);
            //_banner = new ImageIdentifierVM();
        }



        public void Refresh()
        {
            IsVisible = _isVisible;
            IsVisibleImage = _isVisibleImage;
            //IsHidden = _isHidden;
            HeroName = _heroName;
            AlphaFactor = _alphaFactor;
            FontSize = _fontSize;

            VoiceLineText = _voiceLineText;

            //IconSize = _iconSize;

            //ScreenPositionX = _screenPositionX;
            //ScreenPositionY = _screenPositionY;
        }

        public void OnMissionModeChanged(Mission mission)
        {
            //if (mission == null) return;
            //IsVisible = (mission.Mode is MissionMode.Battle or MissionMode.Stealth);

            //IsVisibleImage = IsVisible;

            //LTLogger.IMRed("OnMissionModeChanged: " + mission.Mode.ToString());
            //LTLogger.IMRed("   IsVisible: " + _isVisible.ToString());

            //IsHidden = !IsVisible;
        }







        [DataSourceProperty]
        public string HeroName
        {
            get
            {
                return this._heroName;
            }
            set
            {
                if (value != this._heroName)
                {
                    this._heroName = value;
                    base.OnPropertyChangedWithValue(value, "HeroName");
                }
            }
        }


        [DataSourceProperty]
        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }
            set
            {
                if (value != this._isVisible)
                {
                    this._isVisible = value;
                    base.OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }


        [DataSourceProperty]
        public float ScreenPositionX
        {
            get
            {
                return this._screenPositionX;
            }
            set
            {
                if (value != this._screenPositionX)
                {
                    this._screenPositionX = value;
                    base.OnPropertyChangedWithValue(value, "ScreenPositionX");
                }
            }
        }

        [DataSourceProperty]
        public float ScreenPositionY
        {
            get
            {
                return this._screenPositionY;
            }
            set
            {
                if (value != this._screenPositionY)
                {
                    this._screenPositionY = value;
                    base.OnPropertyChangedWithValue(value, "ScreenPositionY");
                }
            }
        }


        [DataSourceProperty]
        public float AlphaFactor
        {
            get
            {
                return this._alphaFactor;
            }
            set
            {
                if (value != this._alphaFactor)
                {
                    this._alphaFactor = value;
                    base.OnPropertyChangedWithValue(value, "AlphaFactor");
                }
            }
        }

        [DataSourceProperty]
        public int FontSize
        {
            get
            {
                return this._fontSize;
            }
            set
            {
                if (value != this._fontSize)
                {
                    this._fontSize = value;
                    base.OnPropertyChangedWithValue(value, "FontSize");
                }
            }
        }


        [DataSourceProperty]
        public float Width
        {
            get
            {
                return this._width;
            }
            set
            {
                bool flag = this._width != value;
                if (flag)
                {
                    this._width = value;
                    base.OnPropertyChangedWithValue(value, "Width");
                }
            }
        }

        public float Height
        {
            get
            {
                return this._height;
            }
            set
            {
                bool flag = this._height != value;
                if (flag)
                {
                    this._height = value;
                    base.OnPropertyChangedWithValue(value, "Height");
                }
            }
        }

        public string Color
        {
            get
            {
                return this._color;
            }
            set
            {
                bool flag = this._color != value;
                if (flag)
                {
                    this._color = value;
                    base.OnPropertyChangedWithValue(value, "Color");
                }
            }
        }



        // Image + text at the corner

        [DataSourceProperty]
        public bool IsVisibleImage
        {
            get
            {
                return this._isVisibleImage;
            }
            set
            {
                if (value != this._isVisibleImage)
                {
                    this._isVisibleImage = value;
                    base.OnPropertyChangedWithValue(value, "IsVisibleImage");
                }
            }
        }

        //[DataSourceProperty]
        //public ImageIdentifierVM Banner
        //{
        //    get => _banner;
        //    set
        //    {
        //        if (value != _banner)
        //        {
        //            _banner = value;
        //            OnPropertyChangedWithValue<ImageIdentifierVM>(value, "Banner");
        //        }
        //    }
        //}


        [DataSourceProperty]
        public CharacterImageIdentifierVM ImageIdentifier
        {
            get => _imageIdentifier;
            set
            {
                _imageIdentifier = value;
                //OnPropertyChanged("ImageIdentifier");
                base.OnPropertyChangedWithValue<CharacterImageIdentifierVM>(value, "ImageIdentifier");
            }
        }


        [DataSourceProperty]
        public string VoiceLineText
        {
            get
            {
                return this._voiceLineText;
            }
            set
            {
                if (value != this._voiceLineText)
                {
                    this._voiceLineText = value;
                    base.OnPropertyChangedWithValue(value, "VoiceLineText");
                }
            }
        }

    }
}
