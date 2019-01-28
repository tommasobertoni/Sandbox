using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib.Remote
{
    /// <summary>
    /// This is quite hacky (but works :).
    /// </summary>
    [Serializable]
    internal class SerializableTask<TResult> : Task<TResult>, ISerializable
    {
        /// <summary>
        /// Constructor that takes a result.
        /// This constructor retrieves the internal constructor of Task<T> that is used to create a completed Task.
        /// By invoking this internal constructor, this instance of SerializableTask acts exactly like a completed Task.
        /// </summary>
        /// <param name="result">The result of this Task.</param>
        public SerializableTask(TResult result) : base(() => default /* This value is actually never used! */)
        {
            var taskType = typeof(Task<TResult>);

            var constructorFromResult = taskType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(TResult) },
                null);

            constructorFromResult.Invoke(this, new object[] { result }); // == new Task(42)
        }

        /// <summary>
        /// This constructor is a helper to create a new instance by retrieving synchronously the result of a Task.
        /// </summary>
        public SerializableTask(Task<TResult> task) : this(task.GetAwaiter().GetResult())
        {
        }

        /// <summary>
        /// This constructor gets only invoked when an instance of SerializableTask is created throu binary deserialization.
        /// The result value is stored in the SerializationInfo.
        /// </summary>
        protected SerializableTask(SerializationInfo info, StreamingContext context) : this(GetResultFromSerializationInfo(info))
        {
        }

        /// <summary>
        /// Helper method to extract the result value.
        /// </summary>
        private static TResult GetResultFromSerializationInfo(SerializationInfo info) =>
            (TResult)(info.GetValue(nameof(Result), typeof(TResult)) ?? default(TResult));

        /// <summary>
        /// This method comes from the ISerializable interface and is invoked by the binary formatter when this instance is getting serialized.
        /// SerializationInfo should contain all the values that will be sent by .NET Remoting to the Client app, and that will be used to
        /// recreate a SerializableTask instance.
        /// </summary>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) =>
            info.AddValue(nameof(Result), this.Result, typeof(TResult));
    }

    [Serializable]
    internal class SerializableTask : Task, ISerializable
    {
        /// <summary>
        /// This constructor retrieves the internal constructor of Task that is used to create a completed Task.
        /// By invoking this internal constructor, this instance of SerializableTask acts exactly like a completed Task.
        /// </summary>
        public SerializableTask() : base(() => { })
        {
            var taskType = typeof(Task);

            var constructorFromResult = taskType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(bool), typeof(TaskCreationOptions), typeof(CancellationToken) },
                null);
            
            constructorFromResult.Invoke(this, new object[] { false, (TaskCreationOptions)16384, default(CancellationToken) }); // == Task.CompletedTask
        }

        /// <summary>
        /// This constructor is a helper to create a new instance by synchronously waiting the Task to complete.
        /// </summary>
        public SerializableTask(Task task) : this()
        {
            task.Wait();
        }

        /// <summary>
        /// This constructor gets only invoked when an instance of SerializableTask is created throu binary deserialization.
        /// No data will be stored in the SerializationInfo.
        /// </summary>
        protected SerializableTask(SerializationInfo info, StreamingContext context) : this() { }

        /// <summary>
        /// This method comes from the ISerializable interface and is invoked by the binary formatter when this instance is getting serialized.
        /// SerializationInfo will be empty, since this task doesn't have a value.
        /// </summary>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
