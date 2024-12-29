using JZK.Framework;
using JZK.Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JZK.Gameplay
{
    public class ThemeDataLoadSystem : PersistentSystem<ThemeDataLoadSystem>
    {
        #region PersistentSystem

        private SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.Game, BlockStateUntilFinished = ELoadingState.Game } },
            UpdateAfterLoadingState = ELoadingState.Game,
        };

        public override SystemLoadData LoadData
        {
            get { return _loadData; }
        }

        public override void StartLoading(ELoadingState state)
        {
            base.StartLoading(state);

            Addressables.LoadAssetsAsync<ThemeDataSO>("theme_definition", LoadedAsset).Completed += LoadCompleted;
        }



        #endregion //PersistentSystem


        private Dictionary<ELevelTheme, ThemeDefinition> _themeData_LUT = new();
        public Dictionary<ELevelTheme, ThemeDefinition> ThemeData_LUT => _themeData_LUT;


        #region Load

        void LoadedAsset(ThemeDataSO asset)
        {

        }

        void LoadCompleted(AsyncOperationHandle<IList<ThemeDataSO>> assets)
        {
            if (!assets.IsDone)
            {
                //complain here
                return;
            }

            IList<ThemeDataSO> definitionSOs = assets.Result;
            if (definitionSOs == null)
            {
                //complain here
                return;
            }

            _themeData_LUT.Clear();

            foreach (ThemeDataSO defSO in definitionSOs)
            {
                ThemeDefinition def = defSO.Definition.CreateCopy();
                _themeData_LUT.Add(def.Theme, def);
            }

            FinishLoading(ELoadingState.Game);

        }

        #endregion //Load
    }
}