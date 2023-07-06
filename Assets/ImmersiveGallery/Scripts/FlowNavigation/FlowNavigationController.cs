using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Core;
using Broccollie.UI;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gallery
{
    public class FlowNavigationController : MonoBehaviour
    {
        private const string s_mainItemsKey = "MainItem";
        private const int s_emptyIndex = -1;
        private const string s_select = "select";
        private const string s_play = "play";

        [SerializeField] private SettingEventChannel _settingEventChannel = null;

        [SerializeField] private Canvas _canvas = null;
        [SerializeField] private List<FlowItemPreset> _itemPresets = null;
        [SerializeField] private List<Transform> _itemAnchors = null;
        [SerializeField] private List<Transform> _subItemAnchors = null;
        [SerializeField] private Transform _itemHolder = null;
        [SerializeField] private Transform _selectedItemAnchor = null;
        [SerializeField] private Transform _selectedItemCenterAnchor = null;
        [SerializeField] private Transform _selectedCameraAnchor = null;
        [SerializeField] private int _mainItemCenterIndex = 2; // first index is 0
        [SerializeField] private int _subItemCenterIndex = 3;
        [SerializeField] private float _moveToIndexDelay = 0.2f;

        [SerializeField] private FlowMovementPreset _itemMovePreset = null;
        [SerializeField] private FlowMovementPreset _itemSelectionMovePreset = null;
        [SerializeField] private FlowMovementPreset _cameraSelectionMovePreset = null;

        [SerializeField] private TextMeshProUGUI _itemNameText = null;
        [SerializeField] private TextMeshProUGUI _itemDescriptionText = null;
        [SerializeField] private GameObject _rotateIndicators = null;
        [SerializeField] private ButtonUI _upButton = null;
        [SerializeField] private ButtonUI _downButton = null;
        [SerializeField] private ButtonUI _actionButton = null;
        [SerializeField] private TextMeshProUGUI _actionButtonText = null;
        [SerializeField] private ButtonUI _backButton = null;
        [SerializeField] private ButtonUI _leftButton = null;
        [SerializeField] private ButtonUI _rightButton = null;

        private Dictionary<string, List<FlowItem>> _createdItems = new();
        private Dictionary<string, List<int>> _activeIndexes = new();
        private readonly Dictionary<string, Stack<int>> _topStacks = new();
        private readonly Dictionary<string, Stack<int>> _botStacks = new();

        private string _selectedItemsKey = null;
        private CancellationTokenSource _moveCts = new();

        private IEnumerator _disableItemsCoroutine = null;
        private IEnumerator _moveToIndexCoroutine = null;

        private void Awake()
        {
            _upButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(true, true);
            _downButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(false, true);
            _actionButton.OnClick += (sender, args) => SelectItem();
            _backButton.OnClick += (sender, args) => BackSelectionAsync();
            _leftButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(true, true);
            _rightButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(false, true);

            _backButton.SetActive(false);
            _rotateIndicators.SetActive(false);
            _leftButton.SetActive(false);
            _rightButton.SetActive(false);
        }

        private void OnEnable()
        {
            _settingEventChannel.OnSettingClose += Resume;
            _settingEventChannel.OnSettingOpen += Pause;
        }

        private void OnDisable()
        {
            _settingEventChannel.OnSettingClose -= Resume;
            _settingEventChannel.OnSettingOpen -= Pause;
        }

        #region Subscribers
        private async void MovePositionsByDirectionAsync(bool up, bool updateUI)
        {
            if (!_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> flowItems) ||
                !_activeIndexes.TryGetValue(_selectedItemsKey, out List<int> indexes)) return;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            if ((up && indexes[centerIndex] == flowItems.Count - 1) || (!up && indexes[centerIndex] == 0))
            {
                // error sound?
                return;
            }

            List<Transform> anchors = _selectedItemsKey == s_mainItemsKey ? _itemAnchors : _subItemAnchors;
            if (up)
            {
                if (_topStacks[_selectedItemsKey].TryPop(out int toppop))
                {
                    indexes.Insert(indexes.Count, toppop);
                    flowItems[toppop].SetReady(true, anchors[anchors.Count - 1]);
                }
                else
                    indexes.Insert(indexes.Count, s_emptyIndex);

                int firstActiveIndex = indexes.First();
                if (firstActiveIndex != s_emptyIndex)
                {
                    _botStacks[_selectedItemsKey].Push(firstActiveIndex);
                    flowItems[firstActiveIndex].SetReady(false, anchors[0]);
                }
                indexes.RemoveAt(0);
            }
            else
            {
                if (_botStacks[_selectedItemsKey].TryPop(out int botpop))
                {
                    indexes.Insert(0, botpop);
                    flowItems[botpop].SetReady(true, anchors[0]);
                }
                else
                    indexes.Insert(0, s_emptyIndex);

                int lastActiveIndex = indexes.Last();
                if (lastActiveIndex != s_emptyIndex)
                {
                    _topStacks[_selectedItemsKey].Push(lastActiveIndex);
                    flowItems[lastActiveIndex].SetReady(false, anchors[anchors.Count - 1]);
                }
                indexes.RemoveAt(indexes.Count - 1);
            }

            if (updateUI)
            {
                _itemNameText.text = flowItems[indexes[centerIndex]].InjectedPreset.ItemName;
                ToggleArrowButtons(indexes[centerIndex], flowItems.Count - 1);
            }
            await UpdateActiveItemsPositionAsync(flowItems, indexes, anchors, _itemMovePreset);
        }

        private void SelectItem()
        {
            if (_activeIndexes[s_mainItemsKey][_mainItemCenterIndex] == s_emptyIndex) return;

            int selectedIndex = _activeIndexes[s_mainItemsKey][_mainItemCenterIndex];
            FlowItem selectedItem = _createdItems[s_mainItemsKey][selectedIndex];
            selectedItem.EnableShowcaser(true, _selectedItemAnchor);

            _backButton.SetActive(true);
            _actionButtonText.text = s_play;

            _selectedItemsKey = selectedItem.InjectedPreset.ItemName;
            if (_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> subItems))
            {
                MoveItemsToSelectionPointsAsync(selectedItem, _selectedItemAnchor);

                if (_moveToIndexCoroutine != null)
                    StopCoroutine(_moveToIndexCoroutine);
                _moveToIndexCoroutine = MoveToIndex(0);
                StartCoroutine(_moveToIndexCoroutine);
            }
            else
            {
                MoveItemsToSelectionPointsAsync(selectedItem, _selectedItemCenterAnchor);

                _itemDescriptionText.text = selectedItem.InjectedPreset.ItemDescription;
                _itemDescriptionText.enabled = true;
                _upButton.SetActive(false);
                _downButton.SetActive(false);
                _leftButton.SetActive(false);
                _rightButton.SetActive(false);
            }

            async void MoveItemsToSelectionPointsAsync(FlowItem selectedItem, Transform anchor)
            {
                List<Task> movePositionsTask = new()
                {
                    selectedItem.MoveToAnchorAsync(anchor, _itemSelectionMovePreset),
                    Camera.main.transform.LerpPositionAsync(_selectedCameraAnchor.position, _cameraSelectionMovePreset.PositionDuration, _moveCts.Token, _cameraSelectionMovePreset.PositionCurve)
                };
                await Task.WhenAll(movePositionsTask);
            }
        }

        private async void BackSelectionAsync()
        {
            string previousItemKey = _selectedItemsKey;
            if (_disableItemsCoroutine != null)
                StopCoroutine(_disableItemsCoroutine);
            _disableItemsCoroutine = DisableActiveItems(previousItemKey, _cameraSelectionMovePreset.PositionDuration);
            StartCoroutine(_disableItemsCoroutine);

            _selectedItemsKey = s_mainItemsKey;
            _backButton.SetActive(false);
            _itemDescriptionText.enabled = false;
            _actionButtonText.text = s_select;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            int centerItemIndex = _activeIndexes[_selectedItemsKey][centerIndex];
            ToggleArrowButtons(centerItemIndex, _createdItems[_selectedItemsKey].Count - 1);
            _itemNameText.text = _createdItems[_selectedItemsKey][centerItemIndex].InjectedPreset.ItemName;

            _createdItems[_selectedItemsKey][centerItemIndex].EnableShowcaser(false, null);
            List<Task> movePositionsTask = new()
            {
                UpdateActiveItemsPositionAsync(_createdItems[_selectedItemsKey], _activeIndexes[_selectedItemsKey], _itemAnchors, _itemSelectionMovePreset),
                Camera.main.transform.LerpPositionAsync(Vector3.zero, _cameraSelectionMovePreset.PositionDuration, _moveCts.Token, _cameraSelectionMovePreset.PositionCurve)
            };
            await Task.WhenAll(movePositionsTask);
        }

        private void Pause() => _itemNameText.enabled = false;

        private void Resume() => _itemNameText.enabled = true;

        #endregion

        #region Public Functions
        public async Task InitializeAsync()
        {
            if (_canvas.renderMode != RenderMode.ScreenSpaceCamera)
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;

            await CreateFlowItems();

            InitializeIndexes();
            InitializeStacks();

            if (_moveToIndexCoroutine != null)
                StopCoroutine(_moveToIndexCoroutine);
            _moveToIndexCoroutine = MoveToIndex(0);
            StartCoroutine(_moveToIndexCoroutine);

            async Task CreateFlowItems()
            {
                _selectedItemsKey = s_mainItemsKey;
                _createdItems = new();
                List<FlowItem> mainItems = new();

                await InstantiateItems(_itemPresets, mainItems);
                _createdItems.Add(s_mainItemsKey, mainItems);

                foreach (FlowItemPreset preset in _itemPresets)
                {
                    if (preset.SubFlowItems == null || preset.SubFlowItems.Count <= 0) continue;
                    List<FlowItem> subItems = new();
                    await InstantiateItems(preset.SubFlowItems, subItems);
                    _createdItems.Add(preset.ItemName, subItems);
                }

                async Task InstantiateItems(List<FlowItemPreset> presets, List<FlowItem> items)
                {
                    for (int i = 0; i < presets.Count; i++)
                    {
                        try
                        {
                            AsyncOperationHandle<GameObject> mainloadHandle = presets[i].ItemRef.LoadAssetAsync<GameObject>();
                            await mainloadHandle.Task;
                            if (mainloadHandle.Status != AsyncOperationStatus.Succeeded) continue;

                            GameObject mainGo = Instantiate(mainloadHandle.Result, _itemHolder);
                            mainGo.transform.localPosition = Vector3.zero;
                            mainGo.transform.LookAt(Vector3.forward, Vector3.up);
                            mainGo.SetActive(false);

                            if (mainGo.TryGetComponent(out FlowItem item))
                            {
                                item.Initialize(presets[i], _rotateIndicators);
                                items.Add(item);
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            void InitializeIndexes()
            {
                _mainItemCenterIndex = _mainItemCenterIndex < 0 || _mainItemCenterIndex > _itemAnchors.Count - 1 ? _itemAnchors.Count / 2 : _mainItemCenterIndex;
                _subItemCenterIndex = _subItemCenterIndex < 0 || _subItemCenterIndex > _subItemAnchors.Count - 1 ? _subItemAnchors.Count / 2 : _subItemCenterIndex;

                _activeIndexes = new Dictionary<string, List<int>>();
                List<int> mainItemIndexes = new();
                for (int i = 0; i < _itemAnchors.Count; i++)
                    mainItemIndexes.Add(s_emptyIndex);
                _activeIndexes.Add(s_mainItemsKey, mainItemIndexes);

                foreach (FlowItemPreset preset in _itemPresets)
                {
                    List<int> subItemIndexes = new();
                    for (int i = 0; i < _subItemAnchors.Count; i++)
                        subItemIndexes.Add(s_emptyIndex);
                    _activeIndexes.Add(preset.ItemName, subItemIndexes);
                }
            }

            void InitializeStacks()
            {
                Stack<int> mainTop = new();
                _topStacks.Add(s_mainItemsKey, mainTop);

                Stack<int> mainBot = new();
                _botStacks.Add(s_mainItemsKey, mainBot);

                if (_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> flowItems))
                {
                    for (int i = flowItems.Count - 1; i >= 0; i--)
                        _topStacks[_selectedItemsKey].Push(i);
                }

                foreach (FlowItemPreset preset in _itemPresets)
                {
                    Stack<int> top = new();
                    _topStacks.Add(preset.ItemName, top);

                    Stack<int> bot = new();
                    _botStacks.Add(preset.ItemName, bot);

                    if (_createdItems.TryGetValue(preset.ItemName, out List<FlowItem> subFlowItems))
                    {
                        for (int i = subFlowItems.Count - 1; i >= 0; i--)
                            _topStacks[preset.ItemName].Push(i);
                    }
                }
            }
        }

        public IEnumerator MoveToIndex(int index)
        {
            if (!_activeIndexes.TryGetValue(_selectedItemsKey, out List<int> indexes)) yield return null;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            if (index < 0 || index > _createdItems[_selectedItemsKey].Count - 1) yield return null;

            if (indexes[centerIndex] != index)
            {
                _itemNameText.text = _createdItems[_selectedItemsKey][index].InjectedPreset.ItemName;
                ToggleArrowButtons(index, _createdItems[_selectedItemsKey].Count - 1);
                bool up = TargetDirIsUp(index, centerIndex);
                while (indexes[centerIndex] != index)
                {
                    MovePositionsByDirectionAsync(up, false);
                    yield return new WaitForSeconds(_moveToIndexDelay);
                }
            }
            else
            {
                _itemNameText.text = _createdItems[_selectedItemsKey][centerIndex].InjectedPreset.ItemName;
                ToggleArrowButtons(indexes[centerIndex], _createdItems[_selectedItemsKey].Count - 1);
                List<Transform> anchors = _selectedItemsKey == s_mainItemsKey ? _itemAnchors : _subItemAnchors;
                yield return UpdateActiveItemsPositionAsync(_createdItems[_selectedItemsKey], indexes, anchors, _itemMovePreset);
            }
        }

        #endregion

        private async Task UpdateActiveItemsPositionAsync(List<FlowItem> flowItems, List<int> indexes, List<Transform> anchors, FlowMovementPreset preset)
        {
            List<Task> movePositionsTask = new();
            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == s_emptyIndex) continue;
                movePositionsTask.Add(flowItems[indexes[i]].MoveToAnchorAsync(anchors[i], preset));
            }
            await Task.WhenAll(movePositionsTask);
        }

        private IEnumerator DisableActiveItems(string key, float delay)
        {
            if (!_createdItems.TryGetValue(key, out List<FlowItem> items)) yield break;

            yield return new WaitForSeconds(delay);
            if (key == _selectedItemsKey) yield break;

            List<Transform> anchors = key == s_mainItemsKey ? _itemAnchors : _subItemAnchors;
            foreach (FlowItem item in items)
                item.SetReady(false, anchors[anchors.Count - 1]);
        }

        private void ToggleArrowButtons(int centerItemIndex, int lastItemIndex)
        {
            if (centerItemIndex == lastItemIndex)
            {
                if (_selectedItemsKey == s_mainItemsKey)
                {
                    _downButton.SetActive(true);
                    _upButton.SetActive(false);
                }
                else
                {
                    _rightButton.SetActive(true);
                    _leftButton.SetActive(false);
                    _upButton.SetActive(false);
                    _downButton.SetActive(false);
                }
            }
            else if (centerItemIndex == 0)
            {
                if (_selectedItemsKey == s_mainItemsKey)
                {
                    _downButton.SetActive(false);
                    _upButton.SetActive(true);
                }
                else
                {
                    _rightButton.SetActive(false);
                    _leftButton.SetActive(true);
                    _downButton.SetActive(false);
                    _upButton.SetActive(false);
                }
            }
            else
            {
                if (_selectedItemsKey == s_mainItemsKey)
                {
                    _downButton.SetActive(true);
                    _upButton.SetActive(true);
                }
                else
                {
                    _rightButton.SetActive(true);
                    _leftButton.SetActive(true);
                    _upButton.SetActive(false);
                    _downButton.SetActive(false);
                }
            }
        }

        private bool TargetDirIsUp(int targetIndex, int centerIndex) => _activeIndexes[_selectedItemsKey][centerIndex] - targetIndex < 0;
    }
}
