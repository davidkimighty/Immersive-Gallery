using System;
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

        [SerializeField] private Canvas _canvas = null;
        [SerializeField] private List<FlowItemPreset> _itemPresets = null;
        [SerializeField] private List<Transform> _itemAnchors = null;
        [SerializeField] private List<Transform> _subItemAnchors = null;
        [SerializeField] private Transform _itemHolder = null;
        [SerializeField] private Transform _selectedItemAnchor = null;
        [SerializeField] private Transform _selectedCameraAnchor = null;
        [SerializeField] private int _mainItemCenterIndex = 2; // first index is 0
        [SerializeField] private int _subItemCenterIndex = 3;
        [SerializeField] private float _moveToIndexDelay = 0.7f;

        [SerializeField] private FlowMovementPreset _itemMovePreset = null;
        [SerializeField] private FlowMovementPreset _itemSelectionMovePreset = null;
        [SerializeField] private FlowMovementPreset _cameraSelectionMovePreset = null;

        [SerializeField] private TextMeshProUGUI _itemNameText = null;
        [SerializeField] private TextMeshProUGUI _itemDescriptionText = null;
        [SerializeField] private ButtonUI _upButton = null;
        [SerializeField] private ButtonUI _downButton = null;
        [SerializeField] private ButtonUI _selectButton = null;
        [SerializeField] private ButtonUI _backButton = null;

        private Dictionary<string, List<FlowItem>> _createdItems = null;
        private Dictionary<string, List<int>> _activeIndexes = null;
        private readonly Dictionary<string, Stack<int>> _topStacks = new();
        private readonly Dictionary<string, Stack<int>> _botStacks = new();

        private string _selectedItemsKey = null;
        private CancellationTokenSource _moveCts = new();
        private CancellationTokenSource _moveToIndexCts = new();

        private void Awake()
        {
            _upButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(true, true);
            _downButton.OnClick += (sender, args) => MovePositionsByDirectionAsync(false, true);
            _selectButton.OnClick += (sender, args) => SelectItemAsync();
            _backButton.OnClick += (sender, args) => BackSelectionAsync();

            _backButton.SetActive(false);
        }

        #region Subscribers
        private async void MovePositionsByDirectionAsync(bool up, bool updateArrow)
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

            if (updateArrow)
                ToggleArrowButtons(indexes[centerIndex], flowItems.Count - 1);
            await UpdateActiveItemsPositionAsync(flowItems, indexes, anchors, _itemMovePreset);
        }

        private async void SelectItemAsync()
        {
            if (_activeIndexes[s_mainItemsKey][_mainItemCenterIndex] == s_emptyIndex) return;

            int selectedIndex = _activeIndexes[s_mainItemsKey][_mainItemCenterIndex];
            MoveItemsToSelectionPointsAsync(_createdItems[s_mainItemsKey][selectedIndex]);
            _backButton.SetActive(true);

            _selectedItemsKey = _createdItems[s_mainItemsKey][selectedIndex].InjectedPreset.Name;
            if (_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> subItems))
            {
                try
                {
                    _moveToIndexCts.Cancel();
                    _moveToIndexCts = new();

                    await MoveToIndexAsync(0, _moveToIndexCts.Token);
                }
                catch (OperationCanceledException) { }
            }
            else
            {
                _itemDescriptionText.enabled = true;
                _upButton.SetActive(false);
                _downButton.SetActive(false);
            }

            async void MoveItemsToSelectionPointsAsync(FlowItem selectedItem)
            {
                List<Task> movePositionsTask = new List<Task>
                {
                    selectedItem.MoveToAnchorAsync(_selectedItemAnchor, _itemSelectionMovePreset),
                    Camera.main.transform.LerpPositionAsync(_selectedCameraAnchor.position, _cameraSelectionMovePreset.PositionDuration, _moveCts.Token, _cameraSelectionMovePreset.PositionCurve)
                };
                await Task.WhenAll(movePositionsTask);
            }
        }

        private async void BackSelectionAsync()
        {
            string previousItemKey = _selectedItemsKey;
            DisableActiveItemsAsync(previousItemKey, (int)_cameraSelectionMovePreset.PositionDuration * 1000);

            _selectedItemsKey = s_mainItemsKey;
            _backButton.SetActive(false);
            _itemDescriptionText.enabled = false;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            int centerItemIndex = _activeIndexes[_selectedItemsKey][centerIndex];
            ToggleArrowButtons(centerItemIndex, _createdItems[_selectedItemsKey].Count - 1);

            List<Task> movePositionsTask = new List<Task>
            {
                UpdateActiveItemsPositionAsync(_createdItems[_selectedItemsKey], _activeIndexes[_selectedItemsKey], _itemAnchors, _itemSelectionMovePreset),
                Camera.main.transform.LerpPositionAsync(Vector3.zero, _cameraSelectionMovePreset.PositionDuration, _moveCts.Token, _cameraSelectionMovePreset.PositionCurve)
            };
            await Task.WhenAll(movePositionsTask);
        }

        #endregion

        #region Public Functions
        public async Task InitializeAsync()
        {
            // Items
            _selectedItemsKey = s_mainItemsKey;
            _createdItems = new Dictionary<string, List<FlowItem>>();
            List<FlowItem> mainItems = new List<FlowItem>();

            await CreateFlowItems(_itemPresets, mainItems);
            _createdItems.Add(s_mainItemsKey, mainItems);

            foreach (FlowItemPreset preset in _itemPresets)
            {
                if (preset.SubFlowItems == null || preset.SubFlowItems.Count <= 0) continue;
                List<FlowItem> subItems = new List<FlowItem>();
                await CreateFlowItems(preset.SubFlowItems, subItems);
                _createdItems.Add(preset.Name, subItems);
            }

            // Canvas
            if (_canvas.renderMode != RenderMode.ScreenSpaceCamera)
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;

            // Indexes
            _mainItemCenterIndex = _mainItemCenterIndex < 0 || _mainItemCenterIndex > _itemAnchors.Count - 1 ? _itemAnchors.Count / 2 : _mainItemCenterIndex;
            _subItemCenterIndex = _subItemCenterIndex < 0 || _subItemCenterIndex > _subItemAnchors.Count - 1 ? _subItemAnchors.Count / 2 : _subItemCenterIndex;

            _activeIndexes = new Dictionary<string, List<int>>();
            List<int> mainItemIndexes = new List<int>();
            for (int i = 0; i < _itemAnchors.Count; i++)
                mainItemIndexes.Add(s_emptyIndex);
            _activeIndexes.Add(s_mainItemsKey, mainItemIndexes);

            foreach (FlowItemPreset preset in _itemPresets)
            {
                List<int> subItemIndexes = new List<int>();
                for (int i = 0; i < _subItemAnchors.Count; i++)
                    subItemIndexes.Add(s_emptyIndex);
                _activeIndexes.Add(preset.Name, subItemIndexes);
            }

            // Stacks
            Stack<int> mainTop = new Stack<int>();
            _topStacks.Add(s_mainItemsKey, mainTop);

            Stack<int> mainBot = new Stack<int>();
            _botStacks.Add(s_mainItemsKey, mainBot);

            if (_createdItems.TryGetValue(_selectedItemsKey, out List<FlowItem> flowItems))
            {
                for (int i = flowItems.Count - 1; i >= 0; i--)
                    _topStacks[_selectedItemsKey].Push(i);
            }

            foreach (FlowItemPreset preset in _itemPresets)
            {
                Stack<int> top = new Stack<int>();
                _topStacks.Add(preset.Name, top);

                Stack<int> bot = new Stack<int>();
                _botStacks.Add(preset.Name, bot);

                if (_createdItems.TryGetValue(preset.Name, out List<FlowItem> subFlowItems))
                {
                    for (int i = subFlowItems.Count - 1; i >= 0; i--)
                        _topStacks[preset.Name].Push(i);
                }
            }
            await MoveToIndexAsync(0, _moveToIndexCts.Token);

            async Task CreateFlowItems(List<FlowItemPreset> presets, List<FlowItem> items)
            {
                for (int i = 0; i < presets.Count; i++)
                {
                    AsyncOperationHandle<GameObject> mainloadHandle = presets[i].ItemRef.LoadAssetAsync<GameObject>();
                    await mainloadHandle.Task;
                    if (mainloadHandle.Status != AsyncOperationStatus.Succeeded) continue;

                    GameObject mainGo = Instantiate(mainloadHandle.Result, _itemHolder);
                    mainGo.transform.localPosition = Vector3.zero;
                    mainGo.transform.LookAt(Vector3.forward, Vector3.up);
                    mainGo.SetActive(false);

                    if (mainGo.TryGetComponent<FlowItem>(out FlowItem item))
                    {
                        item.Initialize(presets[i]);
                        items.Add(item);
                    }
                }
            }
        }

        public async Task MoveToIndexAsync(int index, CancellationToken ct)
        {
            if (!_activeIndexes.TryGetValue(_selectedItemsKey, out List<int> indexes)) return;

            int centerIndex = _selectedItemsKey == s_mainItemsKey ? _mainItemCenterIndex : _subItemCenterIndex;
            if (index < 0 || index > _createdItems[_selectedItemsKey].Count - 1) return;

            if (indexes[centerIndex] != index)
            {
                ToggleArrowButtons(index, _createdItems[_selectedItemsKey].Count - 1);
                bool up = TargetDirIsUp(index, centerIndex);
                while (indexes[centerIndex] != index)
                {
                    MovePositionsByDirectionAsync(up, false);
                    await Task.Delay((int)_moveToIndexDelay * 1000, ct);
                }
            }
            else
            {
                ToggleArrowButtons(indexes[centerIndex], _createdItems[_selectedItemsKey].Count - 1);
                List<Transform> anchors = _selectedItemsKey == s_mainItemsKey ? _itemAnchors : _subItemAnchors;
                await UpdateActiveItemsPositionAsync(_createdItems[_selectedItemsKey], indexes, anchors, _itemMovePreset);
            }
        }

        #endregion

        private async Task UpdateActiveItemsPositionAsync(List<FlowItem> flowItems, List<int> indexes, List<Transform> anchors, FlowMovementPreset preset)
        {
            List<Task> movePositionsTask = new List<Task>();
            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == s_emptyIndex) continue;
                movePositionsTask.Add(flowItems[indexes[i]].MoveToAnchorAsync(anchors[i], preset));
            }
            await Task.WhenAll(movePositionsTask);
        }

        private async void DisableActiveItemsAsync(string key, int delayMillisec)
        {
            if (!_createdItems.TryGetValue(key, out List<FlowItem> items)) return;

            await Task.Delay(delayMillisec);
            if (key == _selectedItemsKey) return;

            List<Transform> anchors = key == s_mainItemsKey ? _itemAnchors : _subItemAnchors;
            foreach (FlowItem item in items)
                item.SetReady(false, anchors[anchors.Count - 1]);
        }

        private void ToggleArrowButtons(int centerItemIndex, int lastItemIndex)
        {
            if (centerItemIndex == lastItemIndex)
            {
                _upButton.SetActive(false);
                _downButton.SetActive(true);
            }
            else if (centerItemIndex == 0)
            {
                _downButton.SetActive(false);
                _upButton.SetActive(true);
            }
            else
            {
                _upButton.SetActive(true);
                _downButton.SetActive(true);
            }
        }

        private bool TargetDirIsUp(int targetIndex, int centerIndex) => _activeIndexes[_selectedItemsKey][centerIndex] - targetIndex < 0;
    }
}
