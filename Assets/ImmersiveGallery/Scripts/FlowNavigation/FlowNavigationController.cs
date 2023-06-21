using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Broccollie.UI;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gallery
{
    public class FlowNavigationController : MonoBehaviour
    {
        private const int s_emptyIndex = -1;

        [SerializeField] private Canvas _canvas = null;

        [SerializeField] private List<FlowItemPreset> _itemPresets = null;
        [SerializeField] private List<Transform> _placeholders = null;
        [SerializeField] private Transform _itemHolder = null;
        [SerializeField] private int _centerIndex = 2; // first index is 0
        [SerializeField] private float _moveToIndexSpeed = 0.7f;

        [SerializeField] private TextMeshProUGUI _itemNameText = null;
        [SerializeField] private ButtonUI _upButton = null;
        [SerializeField] private ButtonUI _downButton = null;

        private List<FlowItem> _createdItems = null;
        private List<int> _activeIndexes = new List<int>();
        private Stack<int> _topStack = new Stack<int>();
        private Stack<int> _botStack = new Stack<int>();

        private IEnumerator _moveCoroutine = null;
        
        #region Public Functions
        public async Task InitializeAsync()
        {
            await CreateFlowItems();

            _upButton.OnClick += (sender, args) => MovePositionsByDirection(true);
            _downButton.OnClick += (sender, args) => MovePositionsByDirection(false);

            InitCanvas();
            InitIndexes();

            MovePositionsByIndex(0);

            async Task CreateFlowItems()
            {
                _createdItems = new List<FlowItem>();
                for (int i = 0; i < _itemPresets.Count; i++)
                {
                    AsyncOperationHandle<GameObject> loadHandle = _itemPresets[i].ItemRef.LoadAssetAsync<GameObject>();

                    await loadHandle.Task;
                    if (loadHandle.Status != AsyncOperationStatus.Succeeded) continue;

                    GameObject go = Instantiate(loadHandle.Result, _itemHolder);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.LookAt(Vector3.forward, Vector3.up);
                    go.SetActive(false);

                    if (go.TryGetComponent<FlowItem>(out FlowItem item))
                    {
                        item.Initialize(_itemPresets[i]);
                        _createdItems.Add(item);
                    }
                }
            }

            void InitCanvas()
            {
                if (_canvas.renderMode != RenderMode.ScreenSpaceCamera)
                    _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = Camera.main;
            }

            void InitIndexes()
            {
                _centerIndex = _centerIndex < 0 || _centerIndex > _placeholders.Count - 1 ? _placeholders.Count / 2 : _centerIndex;

                for (int i = 0; i < _placeholders.Count; i++)
                    _activeIndexes.Add(s_emptyIndex);

                for (int i = _createdItems.Count - 1; i >= 0; i--)
                    _topStack.Push(i);
            }
        }

        public void MovePositionsByDirection(bool up)
        {
            if ((!up && _activeIndexes[_centerIndex] == _createdItems.Count - 1) || (up && _activeIndexes[_centerIndex] == 0))
            {
                // error sound?
                return;
            }

            if (up)
                DownwardsReady();
            else
                UpwardsReady();

            UpdateActiveItemsPosition();
            
            void DownwardsReady()
            {
                if (_botStack.TryPop(out int botpop))
                {
                    _activeIndexes.Insert(0, botpop);
                    _createdItems[botpop].MoveToAnchorReady(_placeholders[0]);
                }
                else
                    _activeIndexes.Insert(0, s_emptyIndex);

                int lastActiveIndex = _activeIndexes.Last();
                if (lastActiveIndex != s_emptyIndex)
                {
                    _topStack.Push(lastActiveIndex);
                    _createdItems[lastActiveIndex].DisableItem();
                }
                _activeIndexes.RemoveAt(_activeIndexes.Count - 1);
            }

            void UpwardsReady()
            {
                if (_topStack.TryPop(out int toppop))
                {
                    _activeIndexes.Insert(_activeIndexes.Count, toppop);
                    _createdItems[toppop].MoveToAnchorReady(_placeholders[_placeholders.Count - 1]);
                }
                else
                    _activeIndexes.Insert(_activeIndexes.Count, s_emptyIndex);

                int firstActiveIndex = _activeIndexes.First();
                if (firstActiveIndex != s_emptyIndex)
                {
                    _botStack.Push(firstActiveIndex);
                    _createdItems[firstActiveIndex].DisableItem();
                }
                _activeIndexes.RemoveAt(0);
            }

            void UpdateActiveItemsPosition()
            {
                for (int i = 0; i < _activeIndexes.Count; i++)
                {
                    if (_activeIndexes[i] == s_emptyIndex) continue;
                    _createdItems[_activeIndexes[i]].MoveToPlaceholderAsync(_placeholders[i]);
                }
            }
        }

        public void MovePositionsByIndex(int index)
        {
            if ((index < 0 || index > _createdItems.Count - 1) || _activeIndexes[_centerIndex] == index) return;

            if (_moveCoroutine != null)
                StopCoroutine(_moveCoroutine);

            bool up = _activeIndexes[_centerIndex] - index > 0;
            _moveCoroutine = Moving();
            StartCoroutine(_moveCoroutine);

            IEnumerator Moving()
            {
                while (_activeIndexes[_centerIndex] != index)
                {
                    MovePositionsByDirection(up);
                    yield return new WaitForSeconds(_moveToIndexSpeed);
                }
            }
        }

        #endregion
    }
}
