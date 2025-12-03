import React, { useState, useEffect, memo } from "react";
import { Select, Row, Col } from "antd";
import dayjs from "dayjs";

const { Option } = Select;

const TimePicker = ({ value, onChange }) => {
  const [hour, setHour] = useState(null);
  const [minute, setMinute] = useState(null);
  const [period, setPeriod] = useState("am");

  useEffect(() => {
    if (value) {
      const date = dayjs(value);
      const h = date.hour();
      setHour(h % 12 || 12);
      setMinute(date.minute());
      setPeriod(h >= 12 ? "pm" : "am");
    } else {
      setHour(null);
      setMinute(null);
      setPeriod("am");
    }
  }, [value]);

  const hourOptions = [
    { label: "12", value: 12 },
    ...Array.from({ length: 11 }, (_, i) => ({
      label: String(i + 1).padStart(2, "0"),
      value: i + 1,
    })),
  ];

  const minuteOptions = Array.from({ length: 60 }, (_, i) => ({
    label: String(i).padStart(2, "0"),
    value: i,
  }));

  const periodOptions = [
    { label: "Sáng", value: "am" },
    { label: "Chiều", value: "pm" },
  ];

  const handleHourChange = (val) => {
    setHour(val);
    updateDate(val, minute, period);
  };

  const handleMinuteChange = (val) => {
    setMinute(val);
    updateDate(hour, val, period);
  };

  const handlePeriodChange = (val) => {
    setPeriod(val);
    updateDate(hour, minute, val);
  };

  const updateDate = (h, m, p) => {
    if (h && m !== null && p) {
      let hour24 = p === "pm" ? (h === 12 ? 12 : h + 12) : h === 12 ? 0 : h;
      const newDate = dayjs()
        .set("hour", hour24)
        .set("minute", m)
        .set("second", 0);
      onChange(newDate);
    } else {
      onChange("");
    }
  };

  return (
    <Row gutter={[8, 8]}>
      <Col span={8}>
        <Select
          placeholder="Giờ"
          options={hourOptions}
          value={hour}
          onChange={handleHourChange}
          style={{ width: "100%", height: 40 }}
        />
      </Col>
      <Col span={8}>
        <Select
          placeholder="Phút"
          options={minuteOptions}
          value={minute}
          onChange={handleMinuteChange}
          style={{ width: "100%", height: 40 }}
        />
      </Col>
      <Col span={8}>
        <Select
          options={periodOptions}
          value={period}
          onChange={handlePeriodChange}
          style={{ width: "100%", height: 40 }}
        />
      </Col>
    </Row>
  );
};

export default memo(TimePicker);
