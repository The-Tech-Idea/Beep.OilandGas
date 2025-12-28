using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// State machine engine for process state transitions
    /// </summary>
    public class ProcessStateMachine
    {
        private readonly Dictionary<string, ProcessState> _states;
        private readonly Dictionary<string, List<ProcessTransition>> _transitions;
        private readonly ILogger<ProcessStateMachine>? _logger;

        public ProcessStateMachine(ILogger<ProcessStateMachine>? logger = null)
        {
            _states = new Dictionary<string, ProcessState>();
            _transitions = new Dictionary<string, List<ProcessTransition>>();
            _logger = logger;
        }

        /// <summary>
        /// Register a state in the state machine
        /// </summary>
        public void RegisterState(ProcessState state)
        {
            if (string.IsNullOrEmpty(state.StateId))
            {
                throw new ArgumentException("State ID cannot be null or empty", nameof(state));
            }

            _states[state.StateId] = state;
            _logger?.LogDebug($"Registered state: {state.StateId}");
        }

        /// <summary>
        /// Register a transition in the state machine
        /// </summary>
        public void RegisterTransition(ProcessTransition transition)
        {
            if (string.IsNullOrEmpty(transition.FromStateId) || string.IsNullOrEmpty(transition.ToStateId))
            {
                throw new ArgumentException("Transition must have valid FromStateId and ToStateId", nameof(transition));
            }

            if (!_transitions.ContainsKey(transition.FromStateId))
            {
                _transitions[transition.FromStateId] = new List<ProcessTransition>();
            }

            _transitions[transition.FromStateId].Add(transition);
            _logger?.LogDebug($"Registered transition: {transition.FromStateId} -> {transition.ToStateId}");
        }

        /// <summary>
        /// Get all available transitions from a given state
        /// </summary>
        public List<string> GetAvailableTransitions(string fromStateId)
        {
            if (!_transitions.ContainsKey(fromStateId))
            {
                return new List<string>();
            }

            return _transitions[fromStateId]
                .Select(t => t.ToStateId)
                .ToList();
        }

        /// <summary>
        /// Check if a transition is allowed
        /// </summary>
        public bool CanTransition(string fromStateId, string toStateId)
        {
            if (!_transitions.ContainsKey(fromStateId))
            {
                return false;
            }

            var transition = _transitions[fromStateId]
                .FirstOrDefault(t => t.ToStateId == toStateId);

            if (transition == null)
            {
                return false;
            }

            // Check if from state allows this transition
            if (_states.ContainsKey(fromStateId))
            {
                var fromState = _states[fromStateId];
                if (!fromState.AllowedTransitions.Contains(toStateId))
                {
                    return false;
                }
            }

            // Check transition condition
            return EvaluateTransitionCondition(transition);
        }

        /// <summary>
        /// Execute a state transition
        /// </summary>
        public async Task<bool> ExecuteTransitionAsync(
            string fromStateId,
            string toStateId,
            Dictionary<string, object> contextData,
            string userId)
        {
            if (!CanTransition(fromStateId, toStateId))
            {
                _logger?.LogWarning($"Transition not allowed: {fromStateId} -> {toStateId}");
                return false;
            }

            var transition = _transitions[fromStateId]
                .FirstOrDefault(t => t.ToStateId == toStateId);

            if (transition == null)
            {
                return false;
            }

            try
            {
                // Execute transition actions
                foreach (var action in transition.Actions.OrderBy(a => a.ExecutionOrder))
                {
                    await ExecuteTransitionActionAsync(action, contextData, userId);
                }

                _logger?.LogInformation($"Transition executed: {fromStateId} -> {toStateId} by {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error executing transition: {fromStateId} -> {toStateId}");
                throw;
            }
        }

        /// <summary>
        /// Get state information
        /// </summary>
        public ProcessState? GetState(string stateId)
        {
            return _states.ContainsKey(stateId) ? _states[stateId] : null;
        }

        /// <summary>
        /// Check if a state is terminal
        /// </summary>
        public bool IsTerminalState(string stateId)
        {
            var state = GetState(stateId);
            return state?.IsTerminal ?? false;
        }

        /// <summary>
        /// Evaluate transition condition
        /// </summary>
        private bool EvaluateTransitionCondition(ProcessTransition transition)
        {
            if (transition.Condition == null)
            {
                return true;
            }

            switch (transition.Condition.ConditionType)
            {
                case "ALWAYS":
                    return true;

                case "CONDITIONAL":
                    // Evaluate conditional expression (simplified - would need expression evaluator)
                    return !string.IsNullOrEmpty(transition.Condition.ConditionExpression);

                case "APPROVAL_REQUIRED":
                    // Check if approval exists in context
                    return true; // Would check approval status

                default:
                    return true;
            }
        }

        /// <summary>
        /// Execute a transition action
        /// </summary>
        private async Task ExecuteTransitionActionAsync(
            TransitionAction action,
            Dictionary<string, object> contextData,
            string userId)
        {
            switch (action.ActionType)
            {
                case "UPDATE_ENTITY":
                    // Update entity status (would call entity service)
                    _logger?.LogDebug($"Action: UPDATE_ENTITY");
                    break;

                case "SEND_NOTIFICATION":
                    // Send notification (would call notification service)
                    _logger?.LogDebug($"Action: SEND_NOTIFICATION");
                    break;

                case "EXECUTE_SERVICE":
                    // Execute service method (would call service)
                    _logger?.LogDebug($"Action: EXECUTE_SERVICE");
                    break;

                default:
                    _logger?.LogWarning($"Unknown action type: {action.ActionType}");
                    break;
            }

            await Task.CompletedTask;
        }
    }
}

