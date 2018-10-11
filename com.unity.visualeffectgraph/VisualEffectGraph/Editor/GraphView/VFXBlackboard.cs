using System;
using UnityEngine;
using UnityEngine.Experimental.VFX;

using UnityEditor.VFX;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Text;
using UnityEditor.Graphs;
using UnityEditor.SceneManagement;

#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleEnums;
using UnityEditor.Experimental.GraphView;
#else
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.Experimental.UIElements.GraphView;
#endif

namespace UnityEditor.VFX.UI
{
    class VFXBlackboardPropertyView : VisualElement, IControlledElement<VFXParameterController>
    {
        public VFXBlackboardPropertyView()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        public VFXBlackboardRow owner
        {
            get; set;
        }

        Controller IControlledElement.controller
        {
            get { return owner.controller; }
        }
        public VFXParameterController controller
        {
            get { return owner.controller; }
        }

        PropertyRM m_Property;
        PropertyRM m_MinProperty;
        PropertyRM m_MaxProperty;
        List<PropertyRM> m_SubProperties;
        StringPropertyRM m_TooltipProperty;

        IEnumerable<PropertyRM> allProperties
        {
            get
            {
                var result = Enumerable.Empty<PropertyRM>();

                if (m_ExposedProperty != null)
                    result = result.Concat(Enumerable.Repeat<PropertyRM>(m_ExposedProperty, 1));
                if (m_Property != null)
                    result = result.Concat(Enumerable.Repeat(m_Property, 1));
                if (m_SubProperties != null)
                    result = result.Concat(m_SubProperties);
                if (m_TooltipProperty != null)
                    result = result.Concat(Enumerable.Repeat<PropertyRM>(m_TooltipProperty, 1));
                if (m_RangeProperty != null)
                    result = result.Concat(Enumerable.Repeat<PropertyRM>(m_RangeProperty, 1));
                if (m_MinProperty != null)
                    result = result.Concat(Enumerable.Repeat(m_MinProperty, 1));
                if (m_MaxProperty != null)
                    result = result.Concat(Enumerable.Repeat(m_MaxProperty, 1));

                return result;
            }
        }


        void GetPreferedWidths(ref float labelWidth)
        {
            foreach (var port in allProperties)
            {
                float portLabelWidth = port.GetPreferredLabelWidth() + 5;

                if (labelWidth < portLabelWidth)
                {
                    labelWidth = portLabelWidth;
                }
            }
        }

        void ApplyWidths(float labelWidth)
        {
            foreach (var port in allProperties)
            {
                port.SetLabelWidth(labelWidth);
            }
        }

        void CreateSubProperties(ref int insertIndex, List<int> fieldPath)
        {
            var subControllers = controller.GetSubControllers(fieldPath);

            var subFieldPath = new List<int>();
            int cpt = 0;
            foreach (var subController in subControllers)
            {
                PropertyRM prop = PropertyRM.Create(subController, 85);
                if (prop != null)
                {
                    m_SubProperties.Add(prop);
                    Insert(insertIndex++, prop);
                }
                if (prop == null || !prop.showsEverything)
                {
                    subFieldPath.Clear();
                    subFieldPath.AddRange(fieldPath);
                    subFieldPath.Add(cpt);
                    CreateSubProperties(ref insertIndex, subFieldPath);
                }
                ++cpt;
            }
        }

        BoolPropertyRM m_RangeProperty;
        BoolPropertyRM m_ExposedProperty;

        IPropertyRMProvider m_RangeProvider;

        public new void Clear()
        {
            m_ExposedProperty = null;
            m_RangeProperty = null;
        }

        void IControlledElement.OnControllerEvent(VFXControllerEvent e) {}
        void IControlledElement.OnControllerChanged(ref ControllerChangedEvent e) {}

        public void SelfChange(int change)
        {
            if (change == VFXParameterController.ValueChanged)
            {
                foreach (var prop in allProperties)
                {
                    prop.Update();
                }
                return;
            }

            int insertIndex = 0;

            if (m_ExposedProperty == null)
            {
                m_ExposedProperty = new BoolPropertyRM(new SimplePropertyRMProvider<bool>("Exposed", () => controller.exposed, t => controller.exposed = t), 55);
                Insert(insertIndex++, m_ExposedProperty);
            }
            else
            {
                insertIndex++;
            }

            if (m_Property == null || !m_Property.IsCompatible(controller))
            {
                if (m_Property != null)
                {
                    m_Property.RemoveFromHierarchy();
                }
                m_Property = PropertyRM.Create(controller, 55);
                if (m_Property != null)
                {
                    Insert(insertIndex++, m_Property);

                    if (m_SubProperties != null)
                    {
                        foreach (var prop in m_SubProperties)
                        {
                            prop.RemoveFromHierarchy();
                        }
                    }
                    m_SubProperties = new List<PropertyRM>();
                    List<int> fieldpath = new List<int>();
                    if (!m_Property.showsEverything)
                    {
                        CreateSubProperties(ref insertIndex, fieldpath);
                    }
                    if(m_TooltipProperty == null)
                    {
                        m_TooltipProperty = new StringPropertyRM(new SimplePropertyRMProvider<string>("Tooltip", () => controller.model.tooltip, t => controller.model.tooltip = t), 55);
                    }
                    Insert(insertIndex++, m_TooltipProperty);
                }
                else
                {
                    m_TooltipProperty = null;
                }
            }
            else
            {
                insertIndex += 1 + m_SubProperties.Count;
            }

            if (controller.canHaveRange)
            {
                if (m_MinProperty == null || !m_MinProperty.IsCompatible(controller.minController))
                {
                    if (m_MinProperty != null)
                        m_MinProperty.RemoveFromHierarchy();
                    m_MinProperty = PropertyRM.Create(controller.minController, 55);
                }
                if (m_MaxProperty == null || !m_MaxProperty.IsCompatible(controller.minController))
                {
                    if (m_MaxProperty != null)
                        m_MaxProperty.RemoveFromHierarchy();
                    m_MaxProperty = PropertyRM.Create(controller.maxController, 55);
                }

                if (m_RangeProperty == null)
                {
                    m_RangeProperty = new BoolPropertyRM(new SimplePropertyRMProvider<bool>("Range", () => controller.hasRange, t => controller.hasRange = t), 55);
                }
                Insert(insertIndex++, m_RangeProperty);

                if (controller.hasRange)
                {
                    if (m_MinProperty.parent == null)
                    {
                        Insert(insertIndex++, m_MinProperty);
                        Insert(insertIndex++, m_MaxProperty);
                    }
                }
                else if (m_MinProperty.parent != null)
                {
                    m_MinProperty.RemoveFromHierarchy();
                    m_MaxProperty.RemoveFromHierarchy();
                }
            }
            else
            {
                if (m_MinProperty != null)
                {
                    m_MinProperty.RemoveFromHierarchy();
                    m_MinProperty = null;
                }
                if (m_MaxProperty != null)
                {
                    m_MaxProperty.RemoveFromHierarchy();
                    m_MaxProperty = null;
                }
                if (m_RangeProperty != null)
                {
                    m_RangeProperty.RemoveFromHierarchy();
                    m_RangeProperty = null;
                }
            }


            foreach (var prop in allProperties)
            {
                prop.Update();
            }
        }

        void OnAttachToPanel(AttachToPanelEvent e)
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent e)
        {
            if (panel != null)
            {
                float labelWidth = 70;
                GetPreferedWidths(ref labelWidth);
                ApplyWidths(labelWidth);
            }
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
    }
    class VFXBlackboardField : BlackboardField, IControlledElement<VFXParameterController>
    {
        public VFXBlackboardRow owner
        {
            get; set;
        }

        public VFXBlackboardField() : base()
        {
            RegisterCallback<MouseEnterEvent>(OnMouseHover);
            RegisterCallback<MouseLeaveEvent>(OnMouseHover);

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
#if UNITY_2019_1_OR_NEWER
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", (a) => GetFirstAncestorOfType<VFXView>().DeleteElements(new GraphElement[] { this }), DropdownMenuAction.AlwaysEnabled);
#else
            evt.menu.AppendAction("Rename", (a) => OpenTextEditor(), DropdownMenu.MenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", (a) => GetFirstAncestorOfType<VFXView>().DeleteElements(new GraphElement[] { this }), DropdownMenu.MenuAction.AlwaysEnabled);
#endif

            evt.StopPropagation();
        }

        Controller IControlledElement.controller
        {
            get { return owner.controller; }
        }
        public VFXParameterController controller
        {
            get { return owner.controller; }
        }
        void IControlledElement.OnControllerEvent(VFXControllerEvent e) {}
        void IControlledElement.OnControllerChanged(ref ControllerChangedEvent e) {}

        public void SelfChange()
        {
            if (controller.exposed)
            {
                icon = Resources.Load<Texture2D>("VFX/exposed dot");
            }
            else
            {
                icon = null;
            }
        }

        void OnMouseHover(EventBase evt)
        {
            VFXView view = GetFirstAncestorOfType<VFXView>();
            if (view != null)
            {
                foreach (var parameter in view.graphElements.ToList().OfType<VFXParameterUI>().Where(t => t.controller.parentController == controller))
                {
#if UNITY_2019_1_OR_NEWER
                    if (evt.eventTypeId == MouseEnterEvent.TypeId())
#else
                    if (evt.GetEventTypeId() == MouseEnterEvent.TypeId())
#endif
                        parameter.AddToClassList("hovered");
                    else
                        parameter.RemoveFromClassList("hovered");
                }
            }
        }
    }

    class VFXBlackboardRow : BlackboardRow, IControlledElement<VFXParameterController>
    {
        VFXBlackboardField m_Field;

        VFXBlackboardPropertyView m_Properties;
        public VFXBlackboardRow() : this(new VFXBlackboardField() { name = "vfx-field" }, new VFXBlackboardPropertyView() { name = "vfx-properties" })
        {
            Button button = this.Q<Button>("expandButton");

            if (button != null)
            {
                button.clickable.clicked += OnExpand;
            }

#if UNITY_2019_1_OR_NEWER
            clippingOption = ClippingOption.ClipAndCacheContents;
#else
            clippingOptions = ClippingOptions.ClipAndCacheContents;
#endif
        }

        void OnExpand()
        {
            controller.expanded = expanded;
        }

        public VFXBlackboardField field
        {
            get
            {
                return m_Field;
            }
        }

        private VFXBlackboardRow(VFXBlackboardField field, VFXBlackboardPropertyView property) : base(field, property)
        {
            m_Field = field;
            m_Properties = property;

            m_Field.owner = this;
            m_Properties.owner = this;
        }

        public int m_CurrentOrder;
        public bool m_CurrentExposed;

        void IControlledElement.OnControllerEvent(VFXControllerEvent e) {}
        void IControlledElement.OnControllerChanged(ref ControllerChangedEvent e)
        {
            m_Field.text = controller.exposedName;
            m_Field.typeText = controller.portType != null ? controller.portType.UserFriendlyName() : "null";

            // if the order or exposed change, let the event be caught by the VFXBlackboard
            if (controller.order == m_CurrentOrder && controller.exposed == m_CurrentExposed)
            {
                e.StopPropagation();
            }
            m_CurrentOrder = controller.order;
            m_CurrentExposed = controller.exposed;

            expanded = controller.expanded;

            m_Properties.SelfChange(e.change);

            m_Field.SelfChange();
            RemoveFromClassList("hovered");
        }

        VFXParameterController m_Controller;
        Controller IControlledElement.controller
        {
            get { return m_Controller; }
        }
        public VFXParameterController controller
        {
            get { return m_Controller; }
            set
            {
                if (m_Controller != value)
                {
                    if (m_Controller != null)
                    {
                        m_Controller.UnregisterHandler(this);
                    }
                    m_Controller = value;
                    m_Properties.Clear();

                    if (m_Controller != null)
                    {
                        m_CurrentOrder = m_Controller.order;
                        m_CurrentExposed = m_Controller.exposed;
                        m_Controller.RegisterHandler(this);
                    }
                }
            }
        }
    }
    class VFXBlackboard : Blackboard, IControlledElement<VFXViewController>, IVFXMovable
    {
        VFXViewController m_Controller;
        Controller IControlledElement.controller
        {
            get { return m_Controller; }
        }
        public VFXViewController controller
        {
            get { return m_Controller; }
            set
            {
                if (m_Controller != value)
                {
                    if (m_Controller != null)
                    {
                        m_Controller.UnregisterHandler(this);
                    }
                    Clear();
                    m_Controller = value;

                    if (m_Controller != null)
                    {
                        m_Controller.RegisterHandler(this);
                    }
                }
            }
        }

        new void Clear()
        {
            m_DefaultCategory.Clear();

            foreach (var cat in m_Categories)
            {
                cat.Value.RemoveFromHierarchy();
            }
            m_Categories.Clear();
        }

        VFXView m_View;

        public VFXBlackboard(VFXView view)
        {
            m_View = view;
            editTextRequested = OnEditName;
            addItemRequested = OnAddItem;

            this.scrollable = true;

            SetPosition(BoardPreferenceHelper.LoadPosition(BoardPreferenceHelper.Board.blackboard, defaultRect));

            m_DefaultCategory = new VFXBlackboardCategory() { title = "parameters"};
            Add(m_DefaultCategory);
            m_DefaultCategory.headerVisible = false;

            AddStyleSheetPath("VFXBlackboard");

            RegisterCallback<MouseDownEvent>(OnMouseClick, TrickleDown.TrickleDown);


            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
            RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

#if UNITY_2019_1_OR_NEWER
            focusable = true;
#else
            focusIndex = 0;
#endif

            m_DragIndicator = new VisualElement();

            m_DragIndicator.name = "dragIndicator";
            m_DragIndicator.style.positionType = PositionType.Absolute;
            shadow.Add(m_DragIndicator);

#if UNITY_2019_1_OR_NEWER
            clippingOption = ClippingOption.ClipContents;
#else
            clippingOptions = ClippingOptions.ClipContents;
#endif
            SetDragIndicatorVisible(false);

            Resizer resizer = this.Query<Resizer>();

            shadow.Add(new ResizableElement());

            style.positionType = PositionType.Absolute;

            subTitle = "Parameters";

            resizer.RemoveFromHierarchy();
        }


        void OnKeyDown(KeyDownEvent e)
        {
            if( e.keyCode == KeyCode.F2)
            {
                var graphView = GetFirstAncestorOfType<VFXView>();

                var field = graphView.selection.OfType<VFXBlackboardField>().FirstOrDefault();
                if( field != null)
                {
                    field.OpenTextEditor();
                }
                else
                {
                    var category = graphView.selection.OfType< VFXBlackboardCategory>().FirstOrDefault();

                    if( category != null)
                    {
                        category.OpenTextEditor();
                    }
                }
            }
        }

        private void SetDragIndicatorVisible(bool visible)
        {
            if (visible && (m_DragIndicator.parent == null))
            {
                shadow.Add(m_DragIndicator);
                m_DragIndicator.visible = true;
            }
            else if ((visible == false) && (m_DragIndicator.parent != null))
            {
                shadow.Remove(m_DragIndicator);
            }
        }

        VisualElement m_DragIndicator;


        int InsertionIndex(Vector2 pos)
        {
            VisualElement owner = contentContainer != null ? contentContainer : this;
            Vector2 localPos = this.ChangeCoordinatesTo(owner, pos);

            if (owner.ContainsPoint(localPos))
            {
                int defaultCatIndex = IndexOf(m_DefaultCategory);

                for (int i = defaultCatIndex + 1; i < childCount; ++i)
                {
                    VFXBlackboardCategory cat = ElementAt(i) as VFXBlackboardCategory;
                    if (cat == null)
                    {
                        return i;
                    }

                    Rect rect = cat.layout;

                    if (localPos.y <= (rect.y + rect.height / 2))
                    {
                        return i;
                    }
                }
                return childCount;
            }
            return -1;
        }

        int m_InsertIndex;

        void OnDragUpdatedEvent(DragUpdatedEvent e)
        {
            var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

            if (selection == null)
            {
                SetDragIndicatorVisible(false);
                return;
            }

            if (selection.Any(t => !(t is VFXBlackboardCategory)))
            {
                SetDragIndicatorVisible(false);
                return;
            }

            Vector2 localPosition = e.localMousePosition;

            m_InsertIndex = InsertionIndex(localPosition);

            if (m_InsertIndex != -1)
            {
                float indicatorY = 0;

                if (m_InsertIndex == childCount)
                {
                    if (childCount > 0)
                    {
                        VisualElement lastChild = this[childCount - 1];

                        indicatorY = lastChild.ChangeCoordinatesTo(this, new Vector2(0, lastChild.layout.height + lastChild.style.marginBottom)).y;
                    }
                    else
                    {
                        indicatorY = this.contentRect.height;
                    }
                }
                else
                {
                    VisualElement childAtInsertIndex = this[m_InsertIndex];

                    indicatorY = childAtInsertIndex.ChangeCoordinatesTo(this, new Vector2(0, -childAtInsertIndex.style.marginTop)).y;
                }

                SetDragIndicatorVisible(true);

                m_DragIndicator.style.positionTop =  indicatorY - m_DragIndicator.style.height * 0.5f;

                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
            else
            {
                SetDragIndicatorVisible(false);
            }
            e.StopPropagation();
        }

        public int GetCategoryIndex(VFXBlackboardCategory cat)
        {
            return IndexOf(cat) - IndexOf(m_DefaultCategory) - 1;
        }

        void OnDragPerformEvent(DragPerformEvent e)
        {
            SetDragIndicatorVisible(false);
            var selection = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
            if (selection == null)
            {
                return;
            }

            var category = selection.OfType<VFXBlackboardCategory>().FirstOrDefault();
            if (category == null)
            {
                return;
            }

            if (m_InsertIndex != -1)
            {
                if (m_InsertIndex > IndexOf(category))
                    --m_InsertIndex;
                controller.MoveCategory(category.title, m_InsertIndex - IndexOf(m_DefaultCategory) - 1);
            }

            SetDragIndicatorVisible(false);
            e.StopPropagation();
        }

        void OnDragLeaveEvent(DragLeaveEvent e)
        {
            SetDragIndicatorVisible(false);
        }

        public void ValidatePosition()
        {
            BoardPreferenceHelper.ValidatePosition(this, m_View, defaultRect);
        }

        static readonly Rect defaultRect = new Rect(100, 100, 300, 500);

        void OnMouseClick(MouseDownEvent e)
        {
            m_View.SetBoardToFront(this);
        }

        void OnAddParameter(object parameter)
        {
            var selectedCategory = m_View.selection.OfType<VFXBlackboardCategory>().FirstOrDefault();
            VFXParameter newParam = m_Controller.AddVFXParameter(Vector2.zero, (VFXModelDescriptorParameters)parameter);
            if (selectedCategory != null && newParam != null)
                newParam.category = selectedCategory.title;
        }

        void OnAddItem(Blackboard bb)
        {
            GenericMenu menu = new GenericMenu();


            menu.AddItem(EditorGUIUtility.TrTextContent("Category"), false, OnAddCategory);
            menu.AddSeparator(string.Empty);

            foreach (var parameter in VFXLibrary.GetParameters())
            {
                VFXParameter model = parameter.model as VFXParameter;

                var type = model.type;

                menu.AddItem(EditorGUIUtility.TextContent(type.UserFriendlyName()), false, OnAddParameter, parameter);
            }

            menu.ShowAsContext();
        }

        public void SetCategoryName(VFXBlackboardCategory cat, string newName)
        {
            int index = GetCategoryIndex(cat);

            bool succeeded = controller.SetCategoryName(index, newName);

            if (succeeded)
            {
                m_Categories.Remove(cat.title);
                cat.title = newName;
                m_Categories.Add(newName, cat);
            }
        }

        void OnAddCategory()
        {
            string newCategoryName = EditorGUIUtility.TrTextContent("new category").text;
            int cpt = 1;
            while (controller.graph.UIInfos.categories.Any(t => t.name == newCategoryName))
            {
                newCategoryName = string.Format(EditorGUIUtility.TrTextContent("new category {0}").text, cpt++);
            }

            controller.graph.UIInfos.categories.Add(new VFXUI.CategoryInfo() { name = newCategoryName });
            controller.graph.Invalidate(VFXModel.InvalidationCause.kUIChanged);
        }

        void OnEditName(Blackboard bb, VisualElement element, string value)
        {
            if (element is VFXBlackboardField)
            {
                (element as VFXBlackboardField).controller.exposedName = value;
            }
        }

        public void OnMoveParameter(IEnumerable<VFXBlackboardRow> rows, VFXBlackboardCategory category, int index)
        {
            //TODO sort elements
            foreach (var row in rows)
            {
                controller.SetParametersOrder(row.controller, index++, category == m_DefaultCategory ? "" : category.title);
            }
        }

        public void SetCategoryExpanded(VFXBlackboardCategory category, bool expanded)
        {
            controller.SetCategoryExpanded(category.title, expanded);
        }

        VFXBlackboardCategory m_DefaultCategory;
        Dictionary<string, VFXBlackboardCategory> m_Categories = new Dictionary<string, VFXBlackboardCategory>();


        public VFXBlackboardRow GetRowFromController(VFXParameterController controller)
        {
            VFXBlackboardCategory cat = null;
            VFXBlackboardRow row = null;
            if (string.IsNullOrEmpty(controller.model.category))
            {
                row = m_DefaultCategory.GetRowFromController(controller);
            }
            else if (m_Categories.TryGetValue(controller.model.category, out cat))
            {
                row = cat.GetRowFromController(controller);
            }

            return row;
        }

        Dictionary<string, bool> m_ExpandedStatus = new Dictionary<string, bool>();
        void IControlledElement.OnControllerEvent(VFXControllerEvent e) {}
        void IControlledElement.OnControllerChanged(ref ControllerChangedEvent e)
        {
            if (e.controller == controller || e.controller is VFXParameterController) //optim : reorder only is only the order has changed
            {

                if (e.controller == controller && e.change == VFXViewController.Change.assetName)
                {
                    title = controller.name;
                    return;
                }

                var orderedCategories = controller.graph.UIInfos.categories;
                var newCategories = new List<VFXBlackboardCategory>();

                if (orderedCategories != null)
                {
                    foreach (var catModel in controller.graph.UIInfos.categories)
                    {
                        VFXBlackboardCategory cat = null;
                        if (!m_Categories.TryGetValue(catModel.name, out cat))
                        {
                            cat = new VFXBlackboardCategory() {title = catModel.name };
                            cat.SetSelectable();
                            m_Categories.Add(catModel.name, cat);
                        }
                        m_ExpandedStatus[catModel.name] = !catModel.collapsed;

                        newCategories.Add(cat);
                    }

                    foreach (var category in m_Categories.Keys.Except(orderedCategories.Select(t => t.name)).ToArray())
                    {
                        m_Categories[category].RemoveFromHierarchy();
                        m_Categories.Remove(category);
                        m_ExpandedStatus.Remove(category);
                    }
                }

                var prevCat = m_DefaultCategory;

                foreach (var cat in newCategories)
                {
                    if (cat.parent == null)
                        Insert(IndexOf(prevCat) + 1, cat);
                    else
                        cat.PlaceInFront(prevCat);
                    prevCat = cat;
                }

                var actualControllers = new HashSet<VFXParameterController>(controller.parameterControllers.Where(t => string.IsNullOrEmpty(t.model.category)));
                m_DefaultCategory.SyncParameters(actualControllers);


                foreach (var cat in newCategories)
                {
                    actualControllers = new HashSet<VFXParameterController>(controller.parameterControllers.Where(t => t.model.category == cat.title));
                    cat.SyncParameters(actualControllers);
                    cat.expanded = m_ExpandedStatus[cat.title];
                }
            }
        }

        public override void UpdatePresenterPosition()
        {
            BoardPreferenceHelper.SavePosition(BoardPreferenceHelper.Board.blackboard, GetPosition());
        }

        public void OnMoved()
        {
            BoardPreferenceHelper.SavePosition(BoardPreferenceHelper.Board.blackboard, GetPosition());
        }
    }
}
