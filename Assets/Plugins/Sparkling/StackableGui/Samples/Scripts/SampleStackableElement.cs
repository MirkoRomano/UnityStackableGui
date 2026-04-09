using System;
using System.Collections;
using UnityEngine;

namespace Sparkling.StackableGui.Sample
{
    public class SampleStackableElement : IStackableUIElement
    {
        public virtual string Path => throw new NotImplementedException();
        public virtual bool IsVisible => Instance != null && Instance.activeSelf;
        public virtual bool IsLoaded => throw new NotImplementedException();
        public virtual bool IsAnimating => m_isAnimating;

        public GameObject Instance => m_instance;
        public GameObject Prefab => m_prefab;
        public GameObject Parent => m_parent;

        protected GameObject m_prefab;
        protected GameObject m_parent;
        protected GameObject m_instance;
        protected Animator m_animator;

        private bool m_isAnimating;

        public virtual void Initialize(GameObject prefab, GameObject parent)
        {
            m_prefab = prefab;
            m_parent = parent;
            m_instance = GameObject.Instantiate(prefab, parent.transform);
            m_animator = m_instance.GetComponent<Animator>();
        }

        public virtual void OnPoppedFromStack()
        {
            throw new NotImplementedException();
        }

        public virtual void OnPushedIntoStack()
        {
            throw new NotImplementedException();
        }

        public virtual void Animate(string animationName, Action callback = null)
        {
            if (m_instance == null)
            {
                return;
            }

            if (!SampleStackableGuiDirector.Active)
            {
                return;
            }

            SampleStackableGuiDirector.Instance.StartCoroutine(WaitForAnimation(animationName, callback));
        }

        private IEnumerator WaitForAnimation(string animationName, Action callback)
        {
            m_isAnimating = true;

            m_animator.Play(animationName);

            yield return new WaitUntil(() =>
                m_animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)
            );

            while (true)
            {
                if (!m_animator.enabled || m_animator.IsInTransition(0))
                {
                    break;
                }

                if (m_animator.runtimeAnimatorController == null)
                {
                    break;
                }

                var state = m_animator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName(animationName) && state.normalizedTime >= 1f)
                {
                    break;
                }

                yield return null;
            }

            m_isAnimating = false;
            callback?.Invoke();
        }

        public virtual void SetActive(bool active)
        {
            if (Instance == null)
            {
                Debug.LogError($"No Instance has been found in {nameof(SampleStackableElement)}");
                return;
            }

            Instance.SetActive(active);
        }

        public virtual void Destroy()
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(m_instance);
            }
            else
            {
                GameObject.DestroyImmediate(m_instance);
            }
        }
    }
}