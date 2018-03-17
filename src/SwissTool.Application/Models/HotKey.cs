// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HotKey.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the HotKey type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    using System;

    using SwissTool.Framework.Definitions;
    using SwissTool.Framework.Enums;

    /// <summary>
    /// The hot key class.
    /// </summary>
    public class HotKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HotKey"/> class.
        /// </summary>
        public HotKey()
        {
            this.ModifierKey = HotKeyModifier.None;
            this.SecondModifierKey = HotKeyModifier.None;
            this.Key = Framework.Enums.HotKey.None;
            this.Registered = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HotKey"/> class.
        /// </summary>
        /// <param name="modifierKey">The modifier key.</param>
        /// <param name="secondModifierKey">The second modifier key.</param>
        /// <param name="key">The key value.</param>
        public HotKey(HotKeyModifier modifierKey, HotKeyModifier secondModifierKey, Framework.Enums.HotKey key)
        {
            this.ModifierKey = modifierKey;
            this.SecondModifierKey = secondModifierKey;
            this.Key = key;
            this.Registered = false;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key value.
        /// </value>
        public Framework.Enums.HotKey Key { get; set; }

        /// <summary>
        /// Gets or sets the modifier key.
        /// </summary>
        /// <value>
        /// The modifier key.
        /// </value>
        public HotKeyModifier ModifierKey { get; set; }

        /// <summary>
        /// Gets or sets the second modifier key.
        /// </summary>
        /// <value>
        /// The second modifier key.
        /// </value>
        public HotKeyModifier SecondModifierKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="HotKey"/> is registered.
        /// </summary>
        /// <value>
        ///  <c>true</c> if registered; otherwise, <c>false</c>.
        /// </value>
        public bool Registered { get; set; }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public IExtension Extension { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public IExtensionHotKeyAction Action { get; set; }

        /// <summary>
        /// Creates a hotkey from the definition.
        /// </summary>
        /// <param name="hotKeyDefinition">The hot key definition.</param>
        /// <param name="action">The action.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>The hotkey.</returns>
        public static HotKey FromDefinition(HotKeyDefinition hotKeyDefinition, IExtensionHotKeyAction action = null, IExtension extension = null)
        {
            var definition = hotKeyDefinition.Definition;
            var definitionParts = definition.Split('|');
            var firstModifier = definitionParts[0];
            var secondModifier = definitionParts[1];
            var key = definitionParts[2];

            return new HotKey
            {
                ModifierKey = (HotKeyModifier)Enum.Parse(typeof(HotKeyModifier), firstModifier),
                SecondModifierKey = (HotKeyModifier)Enum.Parse(typeof(HotKeyModifier), secondModifier),
                Key = (Framework.Enums.HotKey)Enum.Parse(typeof(Framework.Enums.HotKey), key),
                Action = action,
                Extension = extension
            };
        }

        /// <summary>
        /// Converts a extension hot key action to a hotkey.
        /// </summary>
        /// <param name="hotKeyAction">The hot key action.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>The hotkey.</returns>
        public static HotKey FromExtensionHotKeyAction(IExtensionHotKeyAction hotKeyAction, IExtension extension = null)
        {
            return new HotKey(
                hotKeyAction.DefaultHotKey.FirstModifier,
                hotKeyAction.DefaultHotKey.SecondModifier,
                hotKeyAction.DefaultHotKey.HotKey)
            {
                Action = hotKeyAction,
                Extension = extension
            };
        }

        /// <summary>
        /// Converts the hotkey to it's definition.
        /// </summary>
        /// <returns>The hotkey definition.</returns>
        public HotKeyDefinition ToDefinition()
        {
            return new HotKeyDefinition { BindingPath = this.GetBindingPath(), Definition = this.GetDefinition() };
        }      

        /// <summary>
        /// Gets the binding path.
        /// </summary>
        /// <returns>The binding path.</returns>
        public string GetBindingPath()
        {
            return $"{this.Extension.Identifier}/{this.Action.Identifier}";
        }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        /// <returns>The definition.</returns>
        public string GetDefinition()
        {
            return $"{Enum.GetName(typeof(HotKeyModifier), this.ModifierKey)}|{Enum.GetName(typeof(HotKeyModifier), this.SecondModifierKey)}|{Enum.GetName(typeof(Framework.Enums.HotKey), this.Key)}";
        }
    }
}
