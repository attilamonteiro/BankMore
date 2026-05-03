<script setup lang="ts">
defineProps<{
  label: string
  modelValue: string | number
  error?: string
  type?: string
  placeholder?: string
}>()

defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>

<template>
  <div class="flex flex-col gap-1">
    <label class="text-sm font-medium text-gray-700">{{ label }}</label>
    <input
      :type="type ?? 'text'"
      :value="modelValue"
      :placeholder="placeholder"
      :class="[
        'rounded-lg border px-3 py-2.5 text-sm shadow-sm transition focus:outline-none focus:ring-2',
        error
          ? 'border-red-400 focus:ring-red-400'
          : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500',
      ]"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
    <p v-if="error" class="text-xs text-red-600">{{ error }}</p>
  </div>
</template>
