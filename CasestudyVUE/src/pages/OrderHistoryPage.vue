<template>
  <div class="text-center">
    <div class="text-h4 text-primary q-mt-lg">Order History</div>
    <div class="text-h6 text-positive" style="margin: 15px 0">
      {{ state.status }}
    </div>

    <q-scroll-area style="height: 55vh">
      <q-card class="q-ma-md">
        <q-list separator>
          <q-item>
            <q-item-section class="col-5 text-h7 text-secondary text-bold"
              >ID</q-item-section
            >
            <q-item-section class="text-h7 text-secondary text-bold"
              >Date</q-item-section
            >
          </q-item>
          <q-item
            clickable
            v-for="order in state.orders"
            :key="order.id"
            @click="selectOrder(order.id)"
          >
            <q-item-section class="col-5">
              {{ order.id }}
            </q-item-section>
            <q-item-section>
              {{ formatDate(order.orderDate) }}
            </q-item-section>
          </q-item>
        </q-list>
      </q-card>
    </q-scroll-area>
  </div>

  <q-dialog
    v-model="state.orderSelected"
    transition-show="rotate"
    transition-hide="rotate"
  >
    <q-card style="width: 90vw">
      <q-card-actions align="right">
        <q-btn flat label="X" color="primary" v-close-popup class="text-h5" />
      </q-card-actions>
      <q-card-section>
        <div class="text-h4 text-secondary text-center">
          Order #{{ state.order[0].orderId }}
        </div>
        <div class="text-subtitle1 text-secondary text-center">
          {{ formatDate(state.order[0].orderDate) }}
        </div>
      </q-card-section>
      <q-card-section class="text-subtitle2 text-center">
        <q-avatar>
          <img :src="`img/cart.jpg`" />
        </q-avatar>
      </q-card-section>
      <q-card-section class="text-subtitle2 text-center">
        <q-scroll-area style="height: 30vh">
          <q-card class="q-ma-md">
            <q-list separator>
              <q-item>
                <q-item-section
                  class="col-5 text-h7 text-secondary text-bold"
                  style="margin: 0"
                  >NAME</q-item-section
                >
                <q-item-section
                  class="text-h7 text-secondary text-bold"
                  style="margin: 0; padding-left: 12px"
                  ><div>QTY</div>
                  <div>
                    <span>S</span> <span>O</span> <span>B</span>
                  </div></q-item-section
                >
                <q-item-section
                  class="text-h7 text-secondary text-bold"
                  style="margin: 0; padding-left: 35px"
                  >EXT</q-item-section
                >
              </q-item>

              <q-item v-for="item in state.order" :key="item.id">
                <q-item-section class="col-5" style="margin: 0">
                  {{ item.productName }}
                </q-item-section>
                <q-item-section style="margin: 0">
                  {{ item.qtySold }}
                  {{ item.qtyOrdered }}
                  {{ item.qtyBackOrdered }}
                </q-item-section>
                <q-item-section style="margin: 0; align-items: end">
                  ${{ item.cost }}
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section class="text-bold" style="align-items: start"
                  >Sub:
                </q-item-section>
                <q-item-section style="align-items: end">{{
                  formatCurrency(state.subtotal)
                }}</q-item-section>
              </q-item>

              <q-item>
                <q-item-section class="text-bold" style="align-items: start"
                  >Tax(13%):
                </q-item-section>
                <q-item-section style="align-items: end">{{
                  formatCurrency(state.taxes)
                }}</q-item-section>
              </q-item>

              <q-item>
                <q-item-section class="text-bold" style="align-items: start"
                  >Total:
                </q-item-section>
                <q-item-section style="align-items: end">{{
                  formatCurrency(state.total)
                }}</q-item-section>
              </q-item>
            </q-list>
          </q-card>
        </q-scroll-area>
      </q-card-section>
      <q-card-section class="text-center text-positive">
        {{ state.dialogStatus }}
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script>
import { reactive, onMounted } from "vue";
import { fetcher } from "../utils/apiutil";
import { formatDate, formatCurrency } from "../utils/formatutils";

export default {
  setup() {
    onMounted(() => {
      loadOrders();
    });

    let state = reactive({
      status: "",
      orders: [],
      order: {},
      totalCalories: 0,
      orderSelected: false,
      dialogStatus: "",
      subtotal: 0,
      taxes: 0,
      total: 0,
    });

    const loadOrders = async () => {
      state.status = "loading orders from server";
      let customer = JSON.parse(sessionStorage.getItem("customer"));
      try {
        state.orders = await fetcher(`order/${customer.email}`, customer.email);

        if (state.orders.length > 0) {
          state.status = `loaded ${state.orders.length} orders`;
        } else {
          state.status = `there are no orders registered`;
        }
      } catch (err) {
        console.log(err);
        state.status = `Error loading orders: ${err}`;
      }
    };

    const selectOrder = async (orderId) => {
      state.dialogStatus = `loading selected order`;
      let customer = JSON.parse(sessionStorage.getItem("customer"));

      try {
        state.order = await fetcher(`order/${orderId}/${customer.email}`);
        state.orderSelected = true;

        state.order.forEach((item) => {
          state.subtotal += item.cost * item.qtyOrdered;
        });

        state.taxes = (state.subtotal * 13) / 100.0;
        state.total = state.subtotal + state.taxes;

        state.dialogStatus = `selected order loaded`;
      } catch (err) {
        console.log(err);
        state.dialogStatus = `Error loading order: ${err}`;
      }
    };

    return {
      state,
      formatDate,
      formatCurrency,
      selectOrder,
    };
  },
};
</script>
