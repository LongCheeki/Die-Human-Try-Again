# Medieval Characters Pack — Getting Started

Low-poly rigged medieval characters for Unity. This guide covers everything you need to start
using them.

## What's included

A five-character medieval starter cast, each with its held props:

- **Knight** — longsword and kite shield
- **Archer** — longbow
- **Wizard** — staff
- **Farmer** — pitchfork
- **Town Guard** — halberd

## Render pipeline

Medieval Characters Pack is **URP only**. All prefabs live in `Prefabs/URP/` and their
materials use Universal Render Pipeline shaders. Make sure your project is set to URP
(Project Settings → Graphics / Quality) — under the Built-in or HD pipelines the materials
render magenta.

## Using the characters

Drag any prefab from `Prefabs/URP/` into your scene. Every character stands in T-pose at
metric scale (about 2 m tall), with its pivot on the ground between the feet and facing +Z,
so it drops onto a floor at Y 0 with no manual offset.

### Animating

Every character shares **one 25-bone skeleton**, built for this pack, whose bones carry
Unity's Humanoid names (`Hips`, `Chest`, `LeftUpperArm`, …). Each FBX is imported as a Unity
**Humanoid** with that skeleton already mapped to Unity's Humanoid avatar, so the rig is
**compatible with Mixamo animations** — and with Starter Assets, Asset Store packs or any
other Humanoid clip — with no retargeting setup:

1. Import your clip with **Rig → Animation Type: Humanoid**.
2. Create an Animator Controller and add the clip as a state.
3. Assign the controller to the prefab's `Animator` (the avatar is already set).

Because the skeleton is identical across the cast, one controller drives every character.
For in-place loops, tick **Bake Into Pose** for Root Transform Rotation / Y / XZ on the clip,
or turn off **Apply Root Motion** on the `Animator`.

### Held props

Each character is one skinned body mesh (`<Name>_Body`) plus its held props as separate named
meshes (e.g. `Knight_Sword`, `Knight_Shield`) — rigid, not skinned, parented under the hand
or forearm bone (`RightHand`, `LeftLowerArm`) so they follow the animation.
Disable a prop's GameObject to show an empty-handed character, swap its mesh for your own, or
move it under another character's hand bone — the bone names are the same on every character.

### Recolouring

Materials are flat colours from one shared palette (`Materials/URP/Mat_<Swatch>`), with no
texture maps. Every character's body has one material slot per colour, so you can retint a
single character by assigning a duplicated material to its slot, or retint every character
using a colour by changing the shared `Mat_<Swatch>` itself.

## Sample scene

`Samples/URP/URPScene.unity` — the five characters lined up on a stage, lit for URP, every
one already wired to the generic `CharacterAnimator.controller` described in the next
section.

Import via **Window → Package Manager → Medieval Characters Pack → Samples**, or open the
scene directly from the path above.

## Make the cast dance — YMCA from Mixamo

Every character in the sample scene is already wired to
`Samples/URP/Animations/CharacterAnimator.controller`, a generic one-state Animator Controller
whose `Default` state is empty, ready for any Humanoid clip. The dance itself isn't included,
because Mixamo's terms don't allow redistributing its clips. Download it yourself (free) and
drop it in:

### 1. Download the animation from Mixamo

1. Go to [mixamo.com](https://www.mixamo.com) and sign in with a free Adobe account.
2. Open the **Animations** tab, search for **YMCA Dance**, and click it. It previews on
   Mixamo's default character — that's fine, only the motion is used.
3. Click **Download** and set:
   - **Format:** FBX
   - **Skin:** Without Skin
   - **Frames per Second:** 30
   - **Keyframe Reduction:** none
4. Click **Download**. You get `Ymca Dance.fbx`.

### 2. Import it as a Humanoid clip

1. Drag `Ymca Dance.fbx` anywhere under your project's `Assets/` folder.
2. Select it, and in the Inspector's **Rig** tab set **Animation Type** to **Humanoid** and
   **Avatar Definition** to **Create From This Model**. Click **Apply**.
3. Switch to the **Animation** tab and, for the clip (Mixamo names it `mixamo.com`):
   - tick **Loop Time**
   - **Root Transform Rotation:** tick **Bake Into Pose**, **Based Upon: Original**
   - **Root Transform Position (Y):** tick **Bake Into Pose**, **Based Upon: Original**
   - **Root Transform Position (XZ):** tick **Bake Into Pose**, **Based Upon: Original**

   Baking the root keeps each dancer on its spot instead of drifting across the stage.
   Click **Apply**.

### 3. Put the clip in the controller

1. Double-click `Samples/URP/Animations/CharacterAnimator.controller` to open the **Animator**
   window.
2. Click the orange **Default** state.
3. In the Project window, expand `Ymca Dance.fbx` (the arrow on its icon) and drag the
   `mixamo.com` clip into the state's **Motion** field in the Inspector.

### 4. Press Play

Open `Samples/URP/URPScene.unity` and press **Play** — every character dances the same clip,
because they all share one skeleton. To make any other character dance, assign
`CharacterAnimator.controller` to its `Animator`.

Until a clip is in the state, characters in Play mode hold Unity's neutral Humanoid pose (arms
half raised, knees slightly bent) — that's expected, not a rig problem.

## Troubleshooting

**Materials render magenta / pink** — the project isn't on the Universal Render Pipeline, or
no URP asset is assigned in Project Settings → Graphics. This pack ships URP materials only.

**A clip won't play, or the character stays in T-pose** — the clip was imported as Generic or
Legacy. Set its **Rig → Animation Type** to **Humanoid** and reassign it in the controller.

**A character slides or turns away while animating** — the clip carries root motion. Tick
**Bake Into Pose** for the root transform on the clip, or turn off **Apply Root Motion** on
the `Animator`.

**Props pass through the body during an animation** — the clip was authored for empty hands.
Hide the prop's GameObject for that animation, or pick a clip suited to a held item.
