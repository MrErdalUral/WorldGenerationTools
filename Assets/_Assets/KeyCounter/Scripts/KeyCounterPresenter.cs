using UnityEngine;
using Zenject;
using R3;
using System;
using System.Linq;
using System.Collections.Generic;

namespace KeyCounter
{
    public class KeyCounterPresenter : IClickCounterPresenter, IInitializable
    {
        private readonly CounterView.Factory _clickCounterViewFactory;
        private readonly ICounterModel _clickCounterModel;
        private readonly CompositeDisposable _disposables;
        private readonly IDictionary<string, CounterView> _keyCounterViewDictionary = new Dictionary<string, CounterView>();

        private readonly KeyCode[] _ignoredKeys = new KeyCode[] { KeyCode.Escape };
        private readonly KeyCode[] _keyCodes = Enum.GetValues(typeof(KeyCode))
                                                 .Cast<KeyCode>()
                                                 .ToArray();

        private DisposableBag _disposableBag;
        private DisposableBag _counterViewsDisposableBag;

        public KeyCounterPresenter(CounterView.Factory clickCounterViewFactory, ICounterModel clickCounterModel, CompositeDisposable disposables)
        {
            _clickCounterViewFactory = clickCounterViewFactory;
            _clickCounterModel = clickCounterModel;
            _disposables = disposables;
        }

        public void Initialize()
        {
            _counterViewsDisposableBag.AddTo(ref _disposableBag);

            Observable.EveryUpdate()
                .Where(_ => Input.anyKeyDown)
                .Select(_ => GetCurrentKeyDown())
                .Where(IsValidKeyCode)
                .Subscribe(IncrementCounterView)
                .AddTo(ref _disposableBag);

            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ =>
                {
                    _clickCounterModel.ResetCounters();
                    _keyCounterViewDictionary.Clear();
                    _counterViewsDisposableBag.Clear();
                })
                .AddTo(ref _disposableBag);


            _disposableBag.AddTo(_disposables);
        }

        private bool IsValidKeyCode(KeyCode keyCode)
        {
            if (_ignoredKeys.Contains(keyCode)) return false;
            return true;
        }

        private void IncrementCounterView(KeyCode keyCode)
        {
            string keyString = keyCode.ToString();
            if (!_keyCounterViewDictionary.ContainsKey(keyString))
            {
                SetupCounterView(keyString);
            }
            _clickCounterModel.AddToCount(keyString, 1);
        }

        private void SetupCounterView(string keyString)
        {
            var view = _clickCounterViewFactory.Create(keyString);
            _clickCounterModel.GetCounter(keyString)
                .Subscribe(count => view.Count = count)
                .AddTo(ref _counterViewsDisposableBag);
            _keyCounterViewDictionary[keyString] = view;
            view.AddTo(ref _counterViewsDisposableBag);
        }


        private KeyCode GetCurrentKeyDown()
        {
            foreach (var t in _keyCodes)
            {
                if (Input.GetKeyDown(t))
                {
                    return t;
                }
            }
            return KeyCode.None;
        }
    }
}