using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.HeatMap.Animation
{
    /// <summary>
    /// Controls animation playback with timing and frame management.
    /// </summary>
    public class AnimationController
    {
        private readonly TimeSeriesAnimation animation;
        private CancellationTokenSource cancellationTokenSource;
        private Task animationTask;
        private DateTime lastFrameTime;
        private double frameInterval;

        /// <summary>
        /// Gets or sets the frames per second for animation playback.
        /// </summary>
        public double FramesPerSecond
        {
            get => 1000.0 / frameInterval;
            set
            {
                frameInterval = Math.Max(1.0, 1000.0 / value);
            }
        }

        /// <summary>
        /// Gets whether the animation is currently playing.
        /// </summary>
        public bool IsPlaying => animationTask != null && !animationTask.IsCompleted;

        /// <summary>
        /// Event raised when a frame should be rendered.
        /// </summary>
        public event EventHandler<AnimationFrameEventArgs> FrameReady;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationController"/> class.
        /// </summary>
        /// <param name="animation">The time series animation to control.</param>
        public AnimationController(TimeSeriesAnimation animation)
        {
            this.animation = animation ?? throw new ArgumentNullException(nameof(animation));
            FramesPerSecond = 10.0; // Default 10 FPS
        }

        /// <summary>
        /// Starts playing the animation.
        /// </summary>
        public void Play()
        {
            if (IsPlaying)
                return;

            animation.Play();
            lastFrameTime = DateTime.Now;
            cancellationTokenSource = new CancellationTokenSource();

            animationTask = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var currentTime = DateTime.Now;
                    var elapsed = (currentTime - lastFrameTime).TotalMilliseconds;

                    if (elapsed >= frameInterval)
                    {
                        // Get current frame with interpolation if needed
                        var frame = animation.GetCurrentFrame();
                        
                        FrameReady?.Invoke(this, new AnimationFrameEventArgs
                        {
                            TimeIndex = animation.CurrentTimeIndex,
                            DataPoints = frame,
                            IsInterpolated = false
                        });

                        // Advance to next frame
                        if (!animation.NextFrame())
                        {
                            // Reached end, stop if not looping
                            if (!animation.Loop)
                            {
                                Stop();
                                break;
                            }
                        }

                        lastFrameTime = currentTime;
                    }

                    // Small delay to prevent CPU spinning
                    await Task.Delay(1, cancellationTokenSource.Token);
                }
            }, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            if (!IsPlaying)
                return;

            cancellationTokenSource?.Cancel();
            animation.Stop();

            try
            {
                animationTask?.Wait(1000);
            }
            catch (AggregateException)
            {
                // Task cancellation expected
            }

            animationTask = null;
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            Stop();
            animation.Pause();
        }

        /// <summary>
        /// Steps forward one frame.
        /// </summary>
        public void StepForward()
        {
            Stop();
            animation.NextFrame();
            var frame = animation.GetCurrentFrame();
            FrameReady?.Invoke(this, new AnimationFrameEventArgs
            {
                TimeIndex = animation.CurrentTimeIndex,
                DataPoints = frame,
                IsInterpolated = false
            });
        }

        /// <summary>
        /// Steps backward one frame.
        /// </summary>
        public void StepBackward()
        {
            Stop();
            animation.PreviousFrame();
            var frame = animation.GetCurrentFrame();
            FrameReady?.Invoke(this, new AnimationFrameEventArgs
            {
                TimeIndex = animation.CurrentTimeIndex,
                DataPoints = frame,
                IsInterpolated = false
            });
        }

        /// <summary>
        /// Jumps to a specific frame.
        /// </summary>
        /// <param name="timeIndex">The time index to jump to.</param>
        public void GoToFrame(int timeIndex)
        {
            Stop();
            animation.GoToFrame(timeIndex);
            var frame = animation.GetCurrentFrame();
            FrameReady?.Invoke(this, new AnimationFrameEventArgs
            {
                TimeIndex = animation.CurrentTimeIndex,
                DataPoints = frame,
                IsInterpolated = false
            });
        }

        /// <summary>
        /// Resets the animation to the first frame.
        /// </summary>
        public void Reset()
        {
            Stop();
            animation.Reset();
            var frame = animation.GetCurrentFrame();
            FrameReady?.Invoke(this, new AnimationFrameEventArgs
            {
                TimeIndex = animation.CurrentTimeIndex,
                DataPoints = frame,
                IsInterpolated = false
            });
        }
    }

    /// <summary>
    /// Event arguments for animation frame events.
    /// </summary>
    public class AnimationFrameEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the current time index.
        /// </summary>
        public int TimeIndex { get; set; }

        /// <summary>
        /// Gets or sets the data points for this frame.
        /// </summary>
        public List<HEAT_MAP_DATA_POINT> DataPoints { get; set; }

        /// <summary>
        /// Gets or sets whether this frame is interpolated.
        /// </summary>
        public bool IsInterpolated { get; set; }
    }
}

