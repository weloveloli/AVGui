// -----------------------------------------------------------------------
// <copyright file="TodoListController.cs" company="Weloveloli">
//     Copyright (c) Weloveloli.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Weloveloli.AVGui.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Chromely.Core.Network;

    /// <summary>
    /// Defines the <see cref="TodoListController" />.
    /// </summary>
    [ControllerProperty(Name = "TodoListController")]
    public class TodoListController : ChromelyController
    {
        /// <summary>
        /// Defines the _lockObj.
        /// </summary>
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Defines the _todoItemList.
        /// </summary>
        private List<TodoItem> _todoItemList;

        /// <summary>
        /// Defines the StartId.
        /// </summary>
        private const int StartId = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoListController"/> class.
        /// </summary>
        public TodoListController()
        {
            _todoItemList = new List<TodoItem>();
        }

        /// <summary>
        /// The GetTodoItems.
        /// </summary>
        /// <param name="request">The request<see cref="IChromelyRequest"/>.</param>
        /// <returns>The <see cref="IChromelyResponse"/>.</returns>
        [RequestAction(RouteKey = "/todolistcontroller/items")]
        public IChromelyResponse GetTodoItems(IChromelyRequest request)
        {
            var parameters = request.Parameters as IDictionary<string, string>;
            var name = string.Empty;
            var id = string.Empty;
            var todo = string.Empty;
            var completed = string.Empty;

            if (parameters != null && parameters.Any())
            {
                if (parameters.ContainsKey("name")) name = parameters["name"] ?? string.Empty;
                if (parameters.ContainsKey("id")) id = parameters["id"] ?? string.Empty;
                if (parameters.ContainsKey("todo")) todo = parameters["todo"] ?? string.Empty;
                if (parameters.ContainsKey("completed")) completed = parameters["completed"] ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return new ChromelyResponse() { RequestId = request.Id, Data = new List<Result>() };
            }

            int identifier = 0;
            int.TryParse(id, out identifier);

            int intCompleted = 0;
            int.TryParse(completed, out intCompleted);
            intCompleted = intCompleted == 1 ? 1 : 0;

            TodoItem todoItem = new TodoItem(identifier, todo, intCompleted);

            var todoItems = new List<TodoItem>();
            switch (name.ToLower())
            {
                case "add":
                    todoItems = GetOrUpdateList(RequestType.Add, todoItem);
                    break;
                case "delete":
                    todoItems = GetOrUpdateList(RequestType.Delete, todoItem);
                    break;
                case "all":
                    todoItems = GetOrUpdateList(RequestType.All, todoItem);
                    break;
                case "allactive":
                    todoItems = GetOrUpdateList(RequestType.AllActive, todoItem);
                    break;
                case "allcompleted":
                    todoItems = GetOrUpdateList(RequestType.AllCompleted, todoItem);
                    break;
                case "clearcompleted":
                    todoItems = GetOrUpdateList(RequestType.ClearCompleted, todoItem);
                    break;
                case "toggleall":
                    todoItems = GetOrUpdateList(RequestType.ToggleAll, todoItem);
                    break;
            }

            return new ChromelyResponse() { RequestId = request.Id, Data = todoItems }; ;
        }

        /// <summary>
        /// The ToggleActive.
        /// </summary>
        /// <param name="queryParameters">The queryParameters<see cref="IDictionary{string, string}"/>.</param>
        [CommandAction(RouteKey = "/todolistcontroller/toggleactive")]
        public void ToggleActive(IDictionary<string, string> queryParameters)
        {
            var id = string.Empty;
            var completed = string.Empty;

            if (queryParameters != null && queryParameters.Any())
            {
                if (queryParameters.ContainsKey("id")) id = queryParameters["id"] ?? string.Empty;
                if (queryParameters.ContainsKey("completed")) completed = queryParameters["completed"] ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            int identifier = 0;
            int.TryParse(id, out identifier);

            int intCompleted = 0;
            int.TryParse(completed, out intCompleted);
            intCompleted = intCompleted == 1 ? 1 : 0;

            TodoItem todoItem = new TodoItem(identifier, string.Empty, intCompleted);

            GetOrUpdateList(RequestType.ToggleItemComplete, todoItem);
        }

        /// <summary>
        /// The GetOrUpdateList.
        /// </summary>
        /// <param name="requestType">The requestType<see cref="RequestType"/>.</param>
        /// <param name="todoItem">The todoItem<see cref="TodoItem"/>.</param>
        /// <returns>The <see cref="List{TodoItem}"/>.</returns>
        private List<TodoItem> GetOrUpdateList(RequestType requestType, TodoItem todoItem)
        {
            lock (_lockObj)
            {
                _todoItemList = _todoItemList ?? new List<TodoItem>();
                switch (requestType)
                {
                    case RequestType.Add:
                        int nextId = !_todoItemList.Any() ? StartId : _todoItemList.Select(x => x.Id).Max() + 1;
                        todoItem.Id = nextId;
                        if (todoItem != null && todoItem.Valid)
                        {
                            _todoItemList.Add(todoItem);
                            return _todoItemList.OrderByDescending(x => x.Id).ToList();
                        }
                        break;

                    case RequestType.Delete:
                        if (todoItem != null && todoItem.Id > 0)
                        {
                            var itemToRemove = _todoItemList.FirstOrDefault(x => x.Id == todoItem.Id);
                            if (itemToRemove != null)
                            {
                                _todoItemList.Remove(itemToRemove);
                                return _todoItemList.OrderByDescending(x => x.Id).ToList();
                            }
                        }
                        break;

                    case RequestType.All:
                        return _todoItemList.OrderByDescending(x => x.Id).ToList();

                    case RequestType.AllActive:
                        return _todoItemList.Where(aa => aa.Completed == 0).OrderByDescending(x => x.Id).ToList();

                    case RequestType.AllCompleted:
                        return _todoItemList.Where(aa => aa.Completed == 1).OrderByDescending(x => x.Id).ToList();

                    case RequestType.ClearCompleted:
                        _todoItemList.RemoveAll(x => x.Completed == 1);
                        return _todoItemList.OrderByDescending(x => x.Id).ToList();

                    case RequestType.ToggleAll:
                        _todoItemList.ForEach(x => x.Completed = todoItem.Completed);
                        return _todoItemList.OrderByDescending(x => x.Id).ToList();

                    case RequestType.ToggleItemComplete:
                        var itemToToggle = _todoItemList.FirstOrDefault(x => x.Id == todoItem.Id);
                        if (itemToToggle != null)
                        {
                            itemToToggle.Completed = todoItem.Completed;
                        }
                        return null;
                }
            }

            return _todoItemList ?? new List<TodoItem>();
        }
    }

    /// <summary>
    /// Defines the RequestType.
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// Defines the Add.
        /// </summary>
        Add,
        /// <summary>
        /// Defines the Delete.
        /// </summary>
        Delete,
        /// <summary>
        /// Defines the All.
        /// </summary>
        All,
        /// <summary>
        /// Defines the AllActive.
        /// </summary>
        AllActive,
        /// <summary>
        /// Defines the AllCompleted.
        /// </summary>
        AllCompleted,
        /// <summary>
        /// Defines the ClearCompleted.
        /// </summary>
        ClearCompleted,
        /// <summary>
        /// Defines the ToggleAll.
        /// </summary>
        ToggleAll,
        /// <summary>
        /// Defines the ToggleItemComplete.
        /// </summary>
        ToggleItemComplete
    }

    /// <summary>
    /// Defines the <see cref="TodoItem" />.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TodoItem"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="todo">The todo<see cref="string"/>.</param>
        /// <param name="completed">The completed<see cref="int"/>.</param>
        public TodoItem(int id, string todo, int completed)
        {
            Id = id;
            Todo = todo;
            Completed = completed;
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Todo.
        /// </summary>
        public string Todo { get; set; }

        /// <summary>
        /// Gets or sets the Completed.
        /// </summary>
        public int Completed { get; set; }

        /// <summary>
        /// Gets or sets the CreatedDate.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether Valid.
        /// </summary>
        public bool Valid
        {
            get
            {
                return Id > 0 && !string.IsNullOrWhiteSpace(Todo);
            }
        }
    }
}
