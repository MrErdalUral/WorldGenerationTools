using UnityEngine;
using Zenject;
using R3;

namespace KeyCounter
{
    public class KeyCounterInstaller : MonoInstaller
    {
        [SerializeField] private Transform _clickCounterParent;
        [SerializeField] private GameObject _clickCounterViewPrefab;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public override void InstallBindings()
        {
            Container.BindFactory<string, CounterView, CounterView.Factory>()
                .FromPoolableMemoryPool<string, CounterView, CounterViewPool>(poolBinder => poolBinder
                    .WithInitialSize(1)
                    .FromComponentInNewPrefab(_clickCounterViewPrefab)
                    .UnderTransform(_clickCounterParent));
            Container.BindInstance(_disposables);
            Container.BindInterfacesTo<KeyCounterPresenter>().AsSingle();
            Container.BindInterfacesTo<CounterModel>().AsSingle();
        }
    }
    class CounterViewPool : MonoPoolableMemoryPool<string, IMemoryPool, CounterView>
    {
    }
}