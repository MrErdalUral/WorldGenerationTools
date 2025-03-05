using UnityEngine;
using Zenject;
using R3;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ClickCounter
{
    public class ClickCounterPresenter : IClickCounterPresenter, IInitializable
    {
        private readonly CounterView.Factory _clickCounterViewFactory;
        private readonly ICounterModel _clickCounterModel;
        private readonly CompositeDisposable _disposables;
        private readonly IDictionary<string, CounterView> _keyCounterViewDictionary = new Dictionary<string, CounterView>();

        private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
                                                 .Cast<KeyCode>()
                                                 .Where(k => ((int)k < (int)KeyCode.Mouse0))
                                                 .ToArray();

        private DisposableBag _disposableBag;

        public ClickCounterPresenter(CounterView.Factory clickCounterViewFactory, ICounterModel clickCounterModel, CompositeDisposable disposables)
        {
            _clickCounterViewFactory = clickCounterViewFactory;
            _clickCounterModel = clickCounterModel;
            _disposables = disposables;            
        }



        public void Initialize()
        {
            var viewLmbCounter = _clickCounterViewFactory.Create("Left Mouse");
            _clickCounterModel.GetCounter("LMB")
                .Subscribe(count => viewLmbCounter.Count = count)
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
               .Where(_ => Input.GetMouseButtonDown(0))
               .Subscribe(_ => _clickCounterModel.AddToCount("LMB", 1))
               .AddTo(ref _disposableBag);

            var viewRmbCounter = _clickCounterViewFactory.Create("Right Mouse");
            _clickCounterModel.GetCounter("RMB")
                .Subscribe(count => viewRmbCounter.Count = count)
                .AddTo(ref _disposableBag);
            Observable.EveryUpdate()
               .Where(_ => Input.GetMouseButtonDown(1))
               .Subscribe(_ => _clickCounterModel.AddToCount("RMB", 1))
               .AddTo(ref _disposableBag);

            Observable.EveryUpdate()
                .Select(_ => GetCurrentKeyDown())
                .Where(keyCode => keyCode != null)
                .Subscribe(keyCode => {
                    string keyString = keyCode.ToString();
                    _clickCounterModel.AddToCount(keyString, 1);
                    if (!_keyCounterViewDictionary.ContainsKey(keyString))
                    {
                        var view = _clickCounterViewFactory.Create(keyString);
                        _clickCounterModel.GetCounter(keyString).Subscribe(count => view.Count = count);
                        _keyCounterViewDictionary[keyString] = view;
                    }
                })
                .AddTo(ref _disposableBag);

            Observable.EveryUpdate()
               .Where(_ => Input.GetKeyDown(KeyCode.Escape))
               .Subscribe(_ => _disposableBag.Dispose())
               .AddTo(ref _disposableBag);

            _disposableBag.AddTo(_disposables);
        }


        private static KeyCode? GetCurrentKeyDown()
        {
            if (!Input.anyKey)
            {
                return null;
            }

            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (Input.GetKey(keyCodes[i]))
                {
                    return keyCodes[i];
                }
            }
            return null;
        }
    }
}