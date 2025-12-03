
export const formatDateTime = (dateTimeString: string) => {
  if (!dateTimeString) return { date: "", time: "", full: "" };
  
  const date = new Date(dateTimeString);
  if (isNaN(date.getTime())) return { date: "", time: "", full: "" };

  const day = date.getDate().toString().padStart(2, "0");
  const month = (date.getMonth() + 1).toString().padStart(2, "0");
  const year = date.getFullYear();
  const hours = date.getHours().toString().padStart(2, "0");
  const minutes = date.getMinutes().toString().padStart(2, "0");

  const dateStr = `${day}/${month}/${year}`;
  const timeStr = `${hours}:${minutes}`;
  const fullStr = `${timeStr} ${dateStr}`;

  return {
    date: dateStr,
    time: timeStr,
    full: fullStr,
  };
};


export const formatDate = (dateString: string): string => {
  if (!dateString) return "";
  
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return "";

  return date.toLocaleDateString("vi-VN", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
};


export const formatDateTimeLocale = (timeString: string): string => {
  if (!timeString) return "";
  
  const date = new Date(timeString);
  if (isNaN(date.getTime())) return "";

  return date.toLocaleString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

export const formatFullDateTime = (dateTimeString: string) => {
  if (!dateTimeString) return { fullDate: "", shortDate: "", time: "" };
  
  const date = new Date(dateTimeString);
  if (isNaN(date.getTime())) return { fullDate: "", shortDate: "", time: "" };

  return {
    fullDate: date.toLocaleDateString("vi-VN", {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
    }),
    shortDate: date.toLocaleDateString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    }),
    time: date.toLocaleTimeString("vi-VN", {
      hour: "2-digit",
      minute: "2-digit",
    }),
  };
};


export const formatTime = (isoString: string): string => {
  if (!isoString) return "";
  
  const date = new Date(isoString);
  if (isNaN(date.getTime())) return "";

  const hours = date.getHours().toString().padStart(2, "0");
  const minutes = date.getMinutes().toString().padStart(2, "0");
  return `${hours}:${minutes}`;
};

export const formatDateShortMonth = (dateString: string): string => {
  if (!dateString) return "";
  
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return "";

  return date.toLocaleDateString("vi-VN", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
};

