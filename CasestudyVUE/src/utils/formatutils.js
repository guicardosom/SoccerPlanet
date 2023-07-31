const formatCurrency = (value) => {
  return value.toLocaleString("en-US", {
    style: "currency",
    currency: "USD",
  });
};

const formatDate = (date) => {
  let d;
  // see if date is coming from server
  date === undefined ? (d = new Date()) : (d = new Date(Date.parse(date))); // from server
  let _day = d.getDate();
  let _month = d.getMonth() + 1;
  let _year = d.getFullYear();
  let _hour = d.getHours();
  let _min = d.getMinutes();
  if (_min < 10) {
    _min = "0" + _min;
  }
  return _year + "-" + _month + "-" + _day;
};

export { formatCurrency, formatDate };
