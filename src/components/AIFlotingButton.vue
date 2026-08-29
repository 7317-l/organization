<template>
  <button class="ai-floating-button" @click="handleClick">
    <span class="ai-icon">AI</span>
    <span class="ai-text">AI助手</span>
  </button>
</template>

<script setup>
const emit = defineEmits(["open"])

const handleClick = () => {
  emit("open")
}
</script>

<style scoped>
.ai-floating-button {
  position: fixed;
  right: 30px;
  bottom: 30px;
  width: 150px;
  height: 58px;
  border: none;
  border-radius: 30px;
  background: linear-gradient(135deg, #1677ff, #409eff);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  cursor: pointer;
  box-shadow: 0 8px 25px rgba(22, 119, 255, 0.35);
  z-index: 9999;
  transition: all 0.25s ease;
  font-size: 17px;
  font-weight: 600;
}

.ai-floating-button:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 30px rgba(22, 119, 255, 0.45);
}

.ai-floating-button:active {
  transform: scale(0.96);
}

.ai-icon {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 15px;
  font-weight: 700;
}

.ai-text {
  white-space: nowrap;
}
</style>