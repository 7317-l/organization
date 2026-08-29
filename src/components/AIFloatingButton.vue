<template>

  <button
    class="ai-floating-button"
    :style="buttonStyle"
    @click="handleClick"
    @mousedown="startDrag"
    @touchstart="startDrag"
    aria-label="打开AI学习助手"
  >

    <!-- 外层光晕 -->
    <span class="ai-glow"></span>

    <!-- 外层旋转环 -->
    <span class="ai-ring"></span>

    <!-- 主按钮 -->
    <span class="ai-button-core">

      <span class="ai-spark spark-1">✦</span>
      <span class="ai-spark spark-2">✧</span>

      <span class="ai-text">
        AI
      </span>

      <span class="ai-dot"></span>

    </span>

    <!-- 悬停提示 -->
    <span class="ai-tooltip">
      AI学习助手
    </span>

  </button>

</template>


<script setup>

const emit = defineEmits([
  "click"
])

import { ref, computed } from "vue"

const x = ref(null)
const y = ref(null)

const dragging = ref(false)

let startX = 0
let startY = 0
let startLeft = 0
let startTop = 0

const buttonStyle = computed(() => {

  if (x.value === null || y.value === null) {

    return {}

  }

  return {

    right: "auto",
    bottom: "auto",
    left: x.value + "px",
    top: y.value + "px"

  }

})


function startDrag(event) {

  const point = event.touches
    ? event.touches[0]
    : event

  dragging.value = false

  startX = point.clientX
  startY = point.clientY

  const rect =
    event.currentTarget.getBoundingClientRect()

  startLeft = rect.left
  startTop = rect.top


  document.addEventListener(
    "mousemove",
    moveDrag
  )

  document.addEventListener(
    "mouseup",
    stopDrag
  )

  document.addEventListener(
    "touchmove",
    moveDrag
  )

  document.addEventListener(
    "touchend",
    stopDrag
  )

}


function moveDrag(event) {

  const point = event.touches
    ? event.touches[0]
    : event


  const moveDistance =
    Math.abs(point.clientX - startX) +
    Math.abs(point.clientY - startY)


  if(moveDistance > 5){

    dragging.value = true

  }


  let left =
    startLeft +
    point.clientX -
    startX


  let top =
    startTop +
    point.clientY -
    startY


  const maxLeft =
    window.innerWidth - 72

  const maxTop =
    window.innerHeight - 72


  left = Math.max(
    0,
    Math.min(left, maxLeft)
  )

  top = Math.max(
    0,
    Math.min(top, maxTop)
  )


  x.value = left
  y.value = top


}


function stopDrag(){

  document.removeEventListener(
    "mousemove",
    moveDrag
  )

  document.removeEventListener(
    "mouseup",
    stopDrag
  )

  document.removeEventListener(
    "touchmove",
    moveDrag
  )

  document.removeEventListener(
    "touchend",
    stopDrag
  )

}


function handleClick(){

  if(!dragging.value){

    emit("click")

  }

}

</script>


<style scoped>

/* =====================================================
   AI 悬浮按钮
   ===================================================== */

.ai-floating-button {

  position: fixed;

  right: 32px;

  bottom: 32px;

  z-index: 2500;

  width: 72px;

  height: 72px;

  padding: 0;

  border: none;

  background: transparent;

  cursor: pointer;

  outline: none;

  display: flex;

  align-items: center;

  justify-content: center;

}


/* =====================================================
   光晕
   ===================================================== */

.ai-glow {

  position: absolute;

  width: 72px;

  height: 72px;

  border-radius: 50%;

  background:
    rgba(200, 22, 29, 0.25);

  filter: blur(12px);

  animation:
    aiGlow 2.8s ease-in-out infinite;

}


/* =====================================================
   外圈
   ===================================================== */

.ai-ring {

  position: absolute;

  width: 66px;

  height: 66px;

  border-radius: 50%;

  border: 1px solid rgba(
    200,
    22,
    29,
    0.35
  );

  animation:
    aiRing 3.5s linear infinite;

}


/* =====================================================
   主按钮
   ===================================================== */

.ai-button-core {

  position: relative;

  width: 58px;

  height: 58px;

  display: flex;

  align-items: center;

  justify-content: center;

  border-radius: 50%;

  background:
    linear-gradient(
      145deg,
      #e53935,
      #c8161d 55%,
      #a50f15
    );

  box-shadow:
    0 8px 25px
    rgba(
      180,
      20,
      30,
      0.38
    ),

    inset 0 1px 0
    rgba(
      255,
      255,
      255,
      0.35
    ),

    inset 0 -4px 8px
    rgba(
      100,
      0,
      0,
      0.18
    );

  transition:
    transform 0.25s ease,
    box-shadow 0.25s ease;

}


/* =====================================================
   AI文字
   ===================================================== */

.ai-text {

  position: relative;

  z-index: 3;

  color: white;

  font-family:
    Arial,
    "Microsoft YaHei",
    sans-serif;

  font-size: 19px;

  font-weight: 900;

  letter-spacing: -1px;

  text-shadow:
    0 2px 5px
    rgba(
      100,
      0,
      0,
      0.25
    );

}


/* =====================================================
   小光点
   ===================================================== */

.ai-dot {

  position: absolute;

  right: 12px;

  top: 11px;

  width: 6px;

  height: 6px;

  border-radius: 50%;

  background: white;

  box-shadow:
    0 0 8px
    rgba(
      255,
      255,
      255,
      0.9
    );

  animation:
    aiDot 1.8s ease-in-out infinite;

}


/* =====================================================
   装饰星星
   ===================================================== */

.ai-spark {

  position: absolute;

  color: white;

  line-height: 1;

  opacity: 0.9;

  pointer-events: none;

}


.spark-1 {

  left: 10px;

  top: 11px;

  font-size: 10px;

  animation:
    sparkle1 2s ease-in-out infinite;

}


.spark-2 {

  right: 9px;

  bottom: 10px;

  font-size: 8px;

  animation:
    sparkle2 2.4s ease-in-out infinite;

}


/* =====================================================
   悬停
   ===================================================== */

.ai-floating-button:hover
.ai-button-core {

  transform:
    translateY(-4px)
    scale(1.08);

  box-shadow:
    0 14px 32px
    rgba(
      180,
      20,
      30,
      0.45
    ),

    inset 0 1px 0
    rgba(
      255,
      255,
      255,
      0.4
    );

}


.ai-floating-button:hover
.ai-glow {

  animation:
    aiGlowHover 1.2s ease-in-out infinite;

}


/* =====================================================
   点击
   ===================================================== */

.ai-floating-button:active
.ai-button-core {

  transform:
    scale(0.94);

}


/* =====================================================
   Tooltip
   ===================================================== */

.ai-tooltip {

  position: absolute;

  right: 82px;

  top: 50%;

  transform:
    translateY(-50%)
    translateX(8px);

  padding:
    8px 13px;

  border-radius: 8px;

  background:
    rgba(
      30,
      30,
      30,
      0.92
    );

  color: white;

  font-size: 13px;

  white-space: nowrap;

  opacity: 0;

  visibility: hidden;

  transition:
    opacity 0.2s ease,
    transform 0.2s ease;

  pointer-events: none;

}


.ai-tooltip::after {

  content: "";

  position: absolute;

  right: -5px;

  top: 50%;

  width: 9px;

  height: 9px;

  transform:
    translateY(-50%)
    rotate(45deg);

  background:
    rgba(
      30,
      30,
      30,
      0.92
    );

}


.ai-floating-button:hover
.ai-tooltip {

  opacity: 1;

  visibility: visible;

  transform:
    translateY(-50%)
    translateX(0);

}


/* =====================================================
   光晕动画
   ===================================================== */

@keyframes aiGlow {

  0%,
  100% {

    transform:
      scale(0.9);

    opacity: 0.45;

  }

  50% {

    transform:
      scale(1.15);

    opacity: 0.8;

  }

}


@keyframes aiGlowHover {

  0%,
  100% {

    transform:
      scale(0.95);

    opacity: 0.55;

  }

  50% {

    transform:
      scale(1.25);

    opacity: 0.95;

  }

}


/* =====================================================
   外圈动画
   ===================================================== */

@keyframes aiRing {

  from {

    transform:
      rotate(0deg);

  }

  to {

    transform:
      rotate(360deg);

  }

}


/* =====================================================
   小点动画
   ===================================================== */

@keyframes aiDot {

  0%,
  100% {

    opacity: 0.4;

    transform:
      scale(0.8);

  }

  50% {

    opacity: 1;

    transform:
      scale(1.2);

  }

}


/* =====================================================
   星星动画
   ===================================================== */

@keyframes sparkle1 {

  0%,
  100% {

    opacity: 0.35;

    transform:
      scale(0.8)
      rotate(0deg);

  }

  50% {

    opacity: 1;

    transform:
      scale(1.25)
      rotate(20deg);

  }

}


@keyframes sparkle2 {

  0%,
  100% {

    opacity: 0.3;

    transform:
      scale(0.7);

  }

  50% {

    opacity: 1;

    transform:
      scale(1.2);

  }

}


/* =====================================================
   移动端
   ===================================================== */

@media (max-width: 600px) {

  .ai-floating-button {

    right: 20px;

    bottom: 20px;

    width: 64px;

    height: 64px;

  }


  .ai-glow {

    width: 64px;

    height: 64px;

  }


  .ai-ring {

    width: 60px;

    height: 60px;

  }


  .ai-button-core {

    width: 52px;

    height: 52px;

  }


  .ai-text {

    font-size: 17px;

  }


  .ai-tooltip {

    display: none;

  }

}

</style>