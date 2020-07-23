#include <stdio.h>

#include "memory.h"
#include "value.h"

void printValue(Value value){
	printf("%g", value);
}

void initValueArray(ValueArray* array) {
	array->count = 0;
	array->capacity = 0;
	array->values = NULL;
}

void freeValueArray(ValueArray* array) {
	FREE_ARRAY(Value, array->values, array->capacity);
	initValueArray(array);
}

void writeValueArray(ValueArray* array, Value value) {
	if (array->capacity < array->count + 1) {
		int oldCapacity = array->capacity;
		array->capacity = GROW_CAPACITY(array->capacity);
		array->values = GROW_ARRAY(Value, array->values, oldCapacity, array->capacity);
	}

	array->values[array->count] = value;
	array->count++;
}